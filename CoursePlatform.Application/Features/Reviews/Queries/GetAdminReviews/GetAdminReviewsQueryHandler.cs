using CoursePlatform.Application.Contracts.Persistence;
using CoursePlatform.Application.Features.Reviews.DTOs;
using CoursePlatform.Application.Features.Reviews.Specifications;
using CoursePlatform.Domain.Entities;
using MediatR;

namespace CoursePlatform.Application.Features.Reviews.Queries.GetAdminReviews;

public class GetAdminReviewsQueryHandler
    : IRequestHandler<GetAdminReviewsQuery, AdminReviewsDto>
{
    private readonly IUnitOfWork _uow;

    public GetAdminReviewsQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<AdminReviewsDto> Handle(
        GetAdminReviewsQuery request,
        CancellationToken ct)
    {
        var spec = new ReviewsForAdminSpec(
            request.CourseId,
            request.Rating);

        var reviews = await _uow.Repository<Review>()
                                .GetAllWithSpecAsync(spec, ct);

        var totalReviews = reviews.Count;
        var averageRating = totalReviews > 0
            ? Math.Round(reviews.Average(r => r.Rating), 1)
            : 0;

        return new AdminReviewsDto
        {
            TotalReviews = totalReviews,
            AverageRating = averageRating,

            Reviews = reviews.Select(r => new AdminReviewItemDto
            {
                Id = r.Id,
                CourseId = r.CourseId,
                CourseTitle = r.Course.Title,
                StudentName = r.Student.FullName,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToList()
        };
    }
}