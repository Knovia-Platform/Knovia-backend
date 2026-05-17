using CoursePlatform.Application.Features.Reviews.Commands.CreateReview;
using CoursePlatform.Application.Features.Reviews.Commands.DeleteReview;
using CoursePlatform.Application.Features.Reviews.Commands.UpdateReview;
using CoursePlatform.Application.Features.Reviews.DTOs;
using CoursePlatform.Application.Features.Reviews.Queries.GetCourseReviews;
using CoursePlatform.Application.Features.Reviews.Queries.GetInstructorReviews;
using CoursePlatform.Application.Features.Reviews.Queries.GetMyReview;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoursePlatform.API.Controllers;

[ApiController]
[Route("api/courses/{courseId:int}/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly ISender _sender;

    public ReviewsController(ISender sender)
        => _sender = sender;

    /// <summary>
    /// Get all reviews for a course with rating summary.
    /// Public endpoint.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CourseReviewsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CourseReviewsDto>> GetReviews(
        int courseId, CancellationToken ct)
        => Ok(await _sender.Send(
            new GetCourseReviewsQuery(courseId), ct));

    /// <summary>
    /// Get my review for this course (null if not reviewed yet).
    /// </summary>
    [HttpGet("my")]
    [Authorize]
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReviewDto?>> GetMyReview(
        int courseId, CancellationToken ct)
        => Ok(await _sender.Send(
            new GetMyReviewQuery(courseId), ct));

    /// <summary>
    /// Create a review. Must be enrolled. One review per course.
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReviewDto>> Create(
        int courseId,
        [FromBody] CreateReviewRequest request,
        CancellationToken ct)
    {
        var command = new CreateReviewCommand(
            courseId, request.Rating, request.Comment);

        var result = await _sender.Send(command, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Update my review.
    /// </summary>
    [HttpPut("{reviewId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReviewDto>> Update(
        int courseId, int reviewId,
        [FromBody] CreateReviewRequest request,
        CancellationToken ct)
    {
        var command = new UpdateReviewCommand(
            reviewId, request.Rating, request.Comment);

        return Ok(await _sender.Send(command, ct));
    }

    /// <summary>
    /// Delete a review. Student can delete own. Admin can delete any.
    /// </summary>
    [HttpDelete("{reviewId:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int courseId, int reviewId,
        CancellationToken ct)
    {
        await _sender.Send(new DeleteReviewCommand(reviewId), ct);
        return NoContent();
    }


}

public record CreateReviewRequest(int Rating, string Comment);