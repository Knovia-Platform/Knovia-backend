using MediatR;

namespace CoursePlatform.Application.Features.Courses.Queries.GetMyCourses;

public record GetMyCoursesForFilterQuery
    : IRequest<IReadOnlyList<CourseFilterItemDto>>;

public class CourseFilterItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
}