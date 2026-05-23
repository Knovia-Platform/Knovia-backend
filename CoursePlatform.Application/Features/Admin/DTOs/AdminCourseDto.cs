namespace CoursePlatform.Application.Features.Admin.DTOs;

public class AdminCourseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int Enrollments { get; set; }
    public double AverageRating { get; set; }
    public int TotalRatings { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
}