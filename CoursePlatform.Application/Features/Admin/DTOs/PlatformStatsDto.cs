namespace CoursePlatform.Application.Features.Admin.DTOs;

public class PlatformStatsDto
{
    // Users
    public int TotalUsers { get; set; }
    public int TotalStudents { get; set; }
    public int TotalInstructors { get; set; }
    public int NewUsersThisMonth { get; set; }

    // Courses
    public int TotalCourses { get; set; }
    public int PublishedCourses { get; set; }
    public int PendingCourses { get; set; }

    // Revenue
    public decimal TotalGMV { get; set; }  // إجمالي المبيعات
    public decimal PlatformRevenue { get; set; }  // 30% للـ Platform
    public decimal InstructorRevenue { get; set; }  // 70% للـ Instructors
    public decimal RevenueThisMonth { get; set; }
    public double RevenueGrowthPercent { get; set; }

    // Enrollments
    public int TotalEnrollments { get; set; }
    public int EnrollmentsThisMonth { get; set; }

    // Reviews
    public int TotalReviews { get; set; }
    public double AverageRating { get; set; }

    // Chart
    public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = [];
}

public class MonthlyRevenueDto
{
    public string Label { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Count { get; set; }
}