namespace CoursePlatform.Application.Features.Reviews.DTOs;

public class AdminReviewsDto
{
    public int TotalReviews { get; set; }
    public double AverageRating { get; set; }
    public List<AdminReviewItemDto> Reviews { get; set; } = [];
}

public class AdminReviewItemDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}