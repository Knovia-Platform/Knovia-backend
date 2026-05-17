using CoursePlatform.Application.Common.Models;
using CoursePlatform.Application.Features.Courses.Commands.ApproveCourse;
using CoursePlatform.Application.Features.Courses.Commands.ArchiveCourse;
using CoursePlatform.Application.Features.Courses.Commands.CreateCourse;
using CoursePlatform.Application.Features.Courses.Commands.DeleteCourse;
using CoursePlatform.Application.Features.Courses.Commands.RejectCourse;
using CoursePlatform.Application.Features.Courses.Commands.SubmitCourseForReview;
using CoursePlatform.Application.Features.Courses.Commands.UnarchiveCourse;
using CoursePlatform.Application.Features.Courses.Commands.UpdateCourse;
using CoursePlatform.Application.Features.Courses.DTOs;
using CoursePlatform.Application.Features.Courses.Queries.GetCourseById;
using CoursePlatform.Application.Features.Courses.Queries.GetMyCourses;
using CoursePlatform.Application.Features.Courses.Queries.GetPendingCourses;
using CoursePlatform.Application.Features.Courses.Queries.GetPublishedCourses;
using CoursePlatform.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoursePlatform.Application.Contracts.Services;

namespace CoursePlatform.API.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private readonly ISender _sender;

    private readonly IFileStorageService _fileStorage;

    public CoursesController(ISender sender, IFileStorageService fileStorage)
    {
        _sender = sender;
        _fileStorage = fileStorage;
    }


    // ─── Public ───────────────────────────────────────────────────────────

    /// <summary>Get published courses with filters and pagination.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Pagination<CourseSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Pagination<CourseSummaryDto>>> GetPublished(
        [FromQuery] CourseQueryParams queryParams,
        CancellationToken ct)
        => Ok(await _sender.Send(new GetPublishedCoursesQuery(queryParams), ct));

    /// <summary>Get course details by ID.</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CourseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CourseDto>> GetById(
        int id, CancellationToken ct)
        => Ok(await _sender.Send(new GetCourseByIdQuery(id), ct));

    // ─── Instructor ───────────────────────────────────────────────────────

    /// <summary>Get my courses (all statuses).</summary>
    [HttpGet("my")]
    [Authorize(Roles = "Instructor")]
    [ProducesResponseType(typeof(IReadOnlyList<CourseSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CourseSummaryDto>>> GetMyCourses(
        CancellationToken ct)
        => Ok(await _sender.Send(new GetMyCoursesQuery(), ct));
// API/Controllers/CoursesController.cs

[HttpPost]
[Authorize(Roles = "Instructor")]
[Consumes("multipart/form-data")]
[ProducesResponseType(typeof(CourseDto), StatusCodes.Status201Created)]
public async Task<ActionResult<CourseDto>> Create(
    [FromForm] CreateCourseRequest request,
    CancellationToken ct)
{
    string? thumbnailUrl = null;

    if (request.Thumbnail is not null)
    {
     // use SaveAsync instead of UploadAsync to get the URL back
        await using var stream = request.Thumbnail.OpenReadStream();

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.Thumbnail.FileName)}";

        thumbnailUrl = await _fileStorage.SaveAsync(
            stream,
            fileName,
            "thumbnails",
            ct);
    }

    var command = new CreateCourseCommand(
        request.Title,
        request.Description,
        request.ShortDescription,
        request.Price,
        request.Level,
        request.Language,
        request.SubCategoryId,
        request.Requirements ?? [],
        request.WhatYouLearn ?? [],
        thumbnailUrl,
        request.PreviewVideoUrl);

    var result = await _sender.Send(command, ct);
    return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
}

[HttpPut("{id:int}")]
[Authorize(Roles = "Instructor")]
[Consumes("multipart/form-data")]
[ProducesResponseType(typeof(CourseDto), StatusCodes.Status200OK)]
public async Task<ActionResult<CourseDto>> Update(
    int id,
    [FromForm] UpdateCourseRequest request,
    CancellationToken ct)
{
    string? thumbnailUrl = null;

    if (request.Thumbnail is not null)
    {
        await using var stream = request.Thumbnail.OpenReadStream();

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.Thumbnail.FileName)}";

        thumbnailUrl = await _fileStorage.SaveAsync(
            stream,
            fileName,
            "thumbnails",
            ct);
    }

    var command = new UpdateCourseCommand(
        id,
        request.Title,
        request.Description,
        request.ShortDescription,
        request.Price,
        request.DiscountPrice,
        request.Level,
        request.Language,
        request.SubCategoryId,
        request.Requirements ?? [],
        request.WhatYouLearn ?? [],
        thumbnailUrl,
        request.PreviewVideoUrl);

    return Ok(await _sender.Send(command, ct));
}


    /// <summary>Delete course (Draft/Rejected only).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Instructor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _sender.Send(new DeleteCourseCommand(id), ct);
        return NoContent();
    }

    /// <summary>Submit course for admin review.</summary>
    [HttpPost("{id:int}/submit")]
    [Authorize(Roles = "Instructor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Submit(int id, CancellationToken ct)
    {
        await _sender.Send(new SubmitCourseForReviewCommand(id), ct);
        return Ok(new { message = "Course submitted for review." });
    }

    /// <summary>Archive a published course.</summary>
    [HttpPut("{id:int}/archive")]
    [Authorize(Roles = "Instructor,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Archive(int id, CancellationToken ct)
    {
        await _sender.Send(new ArchiveCourseCommand(id), ct);
        return Ok(new { message = "Course archived successfully." });
    }

    // ─── Admin ────────────────────────────────────────────────────────────

    /// <summary>Get all courses pending review.</summary>
    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IReadOnlyList<CourseSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CourseSummaryDto>>> GetPending(
        CancellationToken ct)
        => Ok(await _sender.Send(new GetPendingCoursesQuery(), ct));

    /// <summary>Approve course → Published.</summary>
    [HttpPut("{id:int}/approve")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
    {
        await _sender.Send(new ApproveCourseCommand(id), ct);
        return Ok(new { message = "Course approved and published." });
    }

    /// <summary>Reject course with reason.</summary>
    [HttpPut("{id:int}/reject")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reject(
        int id,
        [FromBody] RejectCourseRequest request,
        CancellationToken ct)
    {
        await _sender.Send(new RejectCourseCommand(id, request.Reason), ct);
        return Ok(new { message = "Course rejected." });
    }

    [HttpPut("{id:int}/unarchive")]
    [Authorize(Roles = "Instructor,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unarchive(int id, CancellationToken ct)
    {
        await _sender.Send(new UnarchiveCourseCommand(id), ct);
        return Ok(new
        {
            message = "Course unarchived. It is now in Draft status. " +
                      "Submit it for review when ready to publish again."
        });
    }

}

// ─── Request Models ────────────────────────────────────────────────────────
// API/Controllers/CoursesController.cs
public class CreateCourseRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public CourseLevel Level { get; set; }
    public string Language { get; set; } = "English";
    public int SubCategoryId { get; set; }
    public List<string>? Requirements { get; set; }
    public List<string>? WhatYouLearn { get; set; }
    public IFormFile? Thumbnail { get; set; }  // ← File هنا بس
    public string? PreviewVideoUrl { get; set; }
}

public class UpdateCourseRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public CourseLevel Level { get; set; }
    public string Language { get; set; } = "English";
    public int SubCategoryId { get; set; }
    public List<string>? Requirements { get; set; }
    public List<string>? WhatYouLearn { get; set; }
    public IFormFile? Thumbnail { get; set; }  // ← File هنا بس
    public string? PreviewVideoUrl { get; set; }
}
public record RejectCourseRequest(string Reason);