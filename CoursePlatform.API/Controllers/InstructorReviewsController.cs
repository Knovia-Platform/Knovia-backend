using CoursePlatform.Application.Features.Courses.Queries.GetMyCourses;
using CoursePlatform.Application.Features.Reviews.DTOs;
using CoursePlatform.Application.Features.Reviews.Queries.GetInstructorReviews;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoursePlatform.API.Controllers;

[ApiController]
[Route("api/instructor/reviews")]
[Authorize(Roles = "Instructor")]
public class InstructorReviewsController : ControllerBase
{
    private readonly ISender _sender;

    public InstructorReviewsController(ISender sender)
        => _sender = sender;

    /// <summary>
    /// Get all reviews for my courses.
    /// Use ?courseId=1 to filter by specific course.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(InstructorReviewsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<InstructorReviewsDto>> GetMyReviews(
        [FromQuery] int? courseId,
        CancellationToken ct)
        => Ok(await _sender.Send(
            new GetInstructorReviewsQuery(courseId), ct));

    /// <summary>
    /// Get my courses list for the filter dropdown.
    /// </summary>
    [HttpGet("courses-filter")]
    [ProducesResponseType(typeof(IReadOnlyList<CourseFilterItemDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CourseFilterItemDto>>> GetCoursesFilter(
        CancellationToken ct)
        => Ok(await _sender.Send(
            new GetMyCoursesForFilterQuery(), ct));
}
