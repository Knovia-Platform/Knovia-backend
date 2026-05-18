using CoursePlatform.Application.Contracts.Persistence;
using CoursePlatform.Application.Features.Admin.DTOs;
using CoursePlatform.Application.Features.Admin.Specifications;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;
using MediatR;

namespace CoursePlatform.Application.Features.Admin.Queries.GetPlatformStats;

public class GetPlatformStatsQueryHandler
    : IRequestHandler<GetPlatformStatsQuery, PlatformStatsDto>
{
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _uow;

    public GetPlatformStatsQueryHandler(
        IUserRepository userRepo,
        IUnitOfWork uow)
    {
        _userRepo = userRepo;
        _uow = uow;
    }

    public async Task<PlatformStatsDto> Handle(
        GetPlatformStatsQuery request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(
            now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastMonth = startOfMonth.AddMonths(-1);
        var months = Math.Clamp(request.Months, 1, 24);
        var periodStart = new DateTime(
            now.AddMonths(-months + 1).Year,
            now.AddMonths(-months + 1).Month,
            1, 0, 0, 0, DateTimeKind.Utc);

        // ─── Users ────────────────────────────────────────────────
        var allUsers = await _userRepo.GetAllAsync(ct: ct);
        var userDetails = new List<(AppUser user, IReadOnlyList<string> roles)>();
        foreach (var u in allUsers)
        {
            var roles = await _userRepo.GetRolesAsync(u, ct);
            userDetails.Add((u, roles));
        }

        var totalStudents = userDetails.Count(u =>
            u.roles.Contains("Student"));
        var totalInstructors = userDetails.Count(u =>
            u.roles.Contains("Instructor"));
        var newThisMonth = allUsers.Count(u =>
            u.CreatedAt >= startOfMonth);

        // ─── Courses ──────────────────────────────────────────────
        var allCoursesSpec = new AllCoursesAdminSpec();
        var allCourses = await _uow.Repository<Course>()
                                       .GetAllWithSpecAsync(allCoursesSpec, ct);

        // ─── Revenue ──────────────────────────────────────────────
        var completedOrders = await _uow.Repository<Order>()
            .GetAllWithSpecAsync(new AllCompletedOrdersSpec(), ct);

        var totalRevenue = completedOrders.Sum(o => o.FinalPrice);
        var revenueThisMonth = completedOrders
            .Where(o => o.PaidAt >= startOfMonth).Sum(o => o.FinalPrice);
        var revenueLastMonth = completedOrders
            .Where(o => o.PaidAt >= lastMonth &&
                        o.PaidAt < startOfMonth).Sum(o => o.FinalPrice);

        var revenueGrowth = revenueLastMonth > 0
            ? Math.Round(
                (double)(revenueThisMonth - revenueLastMonth)
                / (double)revenueLastMonth * 100, 1)
            : revenueThisMonth > 0 ? 100.0 : 0.0;

        // ─── Enrollments ──────────────────────────────────────────
        var allEnrollments = await _uow.Repository<Enrollment>()
            .GetAllWithSpecAsync(new AllEnrollmentsSpec(), ct);

        var enrollThisMonth = allEnrollments
            .Count(e => e.EnrolledAt >= startOfMonth);

        // ─── Reviews ──────────────────────────────────────────────
        var allReviews = await _uow.Repository<Review>()
            .GetAllWithSpecAsync(new AllReviewsSpec(), ct);
        var avgRating = allReviews.Any()
            ? Math.Round(allReviews.Average(r => r.Rating), 1) : 0;

        // ─── Monthly Revenue ──────────────────────────────────────
        var ordersInPeriod = completedOrders
            .Where(o => o.PaidAt >= periodStart).ToList();

        var monthlyRevenue = new List<MonthlyRevenueDto>();
        for (var i = 0; i < months; i++)
        {
            var date = periodStart.AddMonths(i);
            var inMonth = ordersInPeriod
                .Where(o =>
                    o.PaidAt?.Year == date.Year &&
                    o.PaidAt?.Month == date.Month)
                .ToList();

            monthlyRevenue.Add(new MonthlyRevenueDto
            {
                Label = date.ToString("MMM yyyy"),
                Amount = inMonth.Sum(o => o.FinalPrice),
                Count = inMonth.Count
            });
        }

        return new PlatformStatsDto
        {
            // Users
            TotalUsers = allUsers.Count,
            TotalStudents = totalStudents,
            TotalInstructors = totalInstructors,
            NewUsersThisMonth = newThisMonth,

            // Courses
            TotalCourses = allCourses.Count,
            PublishedCourses = allCourses.Count(c => c.Status == CourseStatus.Published),
            PendingCourses = allCourses.Count(c => c.Status == CourseStatus.UnderReview),

            // Revenue ← أضيفي
            TotalGMV = totalRevenue,
            PlatformRevenue = Math.Round(totalRevenue * 0.30m, 2),
            InstructorRevenue = Math.Round(totalRevenue * 0.70m, 2),
            RevenueThisMonth = revenueThisMonth,
            RevenueGrowthPercent = revenueGrowth,

            // Enrollments
            TotalEnrollments = allEnrollments.Count,
            EnrollmentsThisMonth = enrollThisMonth,

            // Reviews
            TotalReviews = allReviews.Count,
            AverageRating = avgRating,

            MonthlyRevenue = monthlyRevenue
        };
    }
}