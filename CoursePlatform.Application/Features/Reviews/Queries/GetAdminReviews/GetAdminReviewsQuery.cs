using CoursePlatform.Application.Features.Reviews.DTOs;
using MediatR;

namespace CoursePlatform.Application.Features.Reviews.Queries.GetAdminReviews;

public record GetAdminReviewsQuery(
    int? CourseId = null,
    int? Rating = null
) : IRequest<AdminReviewsDto>;