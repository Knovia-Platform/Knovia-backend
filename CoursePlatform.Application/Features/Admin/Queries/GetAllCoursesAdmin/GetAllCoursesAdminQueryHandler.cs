using CoursePlatform.Application.Contracts.Persistence;
using CoursePlatform.Application.Features.Admin.DTOs;
using CoursePlatform.Application.Features.Admin.Specifications;
using CoursePlatform.Domain.Entities;
using MediatR;

namespace CoursePlatform.Application.Features.Admin.Queries.GetAllCoursesAdmin;

public class GetAllCoursesAdminQueryHandler
    : IRequestHandler<GetAllCoursesAdminQuery, IReadOnlyList<AdminCourseDto>>
{
    private readonly IUnitOfWork _uow;

    public GetAllCoursesAdminQueryHandler(IUnitOfWork uow)
        => _uow = uow;

    public async Task<IReadOnlyList<AdminCourseDto>> Handle(
        GetAllCoursesAdminQuery request, CancellationToken ct)
    {
        var spec = new AllCoursesAdminSpec(request.Status, request.Search);
        var courses = await _uow.Repository<Course>()
                                .GetAllWithSpecAsync(spec, ct);

        return courses.Select(c => new AdminCourseDto
        {
            Id = c.Id,
            Title = c.Title,
            InstructorName = c.Instructor?.FullName ?? string.Empty,
            Status = c.Status.ToString(),
            Price = c.Price,
            Enrollments = c.Enrollments.Count,
            AverageRating = c.AverageRating,
            TotalRatings = c.TotalRatings,
            RejectionReason = c.RejectionReason,
            DiscountPrice = c.DiscountPrice,
            CreatedAt = c.CreatedAt
        }).ToList();
    }
}