using CoursePlatform.Application.Features.Reviews.DTOs;
using CoursePlatform.Application.Features.Reviews.Queries.GetAdminReviews;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoursePlatform.API.Controllers;


[ApiController]
[Route("api/admin/reviews")]
[Authorize(Roles = "Admin")]
public class AdminReviewsController : ControllerBase
{
    private readonly ISender _sender;

    public AdminReviewsController(ISender sender)
        => _sender = sender;

    /// <summary>
    /// Get all reviews in system (admin dashboard).
    /// Filters: courseId, rating
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(AdminReviewsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminReviewsDto>> GetAll(
        [FromQuery] int? courseId,
        [FromQuery] int? rating,
        CancellationToken ct)
        => Ok(await _sender.Send(
            new GetAdminReviewsQuery(courseId, rating), ct));
}

