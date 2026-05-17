// Application/Features/Reviews/Queries/GetInstructorReviews/GetInstructorReviewsQueryHandler.cs
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Contracts.Persistence;
using CoursePlatform.Application.Contracts.Services;
using CoursePlatform.Application.Features.Reviews.DTOs;
using CoursePlatform.Application.Features.Reviews.Specifications;
using CoursePlatform.Domain.Entities;
using MediatR;

namespace CoursePlatform.Application.Features.Reviews.Queries.GetInstructorReviews;

public class GetInstructorReviewsQueryHandler
    : IRequestHandler<GetInstructorReviewsQuery, InstructorReviewsDto>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetInstructorReviewsQueryHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<InstructorReviewsDto> Handle(
        GetInstructorReviewsQuery request, CancellationToken ct)
    {
        var instructorId = _currentUser.UserId
            ?? throw new UnauthorizedException();

        var spec = new ReviewsByInstructorSpec(instructorId, request.CourseId);
        var reviews = await _uow.Repository<Review>()
                                .GetAllWithSpecAsync(spec, ct);

        // Summary
        var totalReviews = reviews.Count;
        var averageRating = totalReviews > 0
            ? Math.Round(reviews.Average(r => r.Rating), 1)
            : 0.0;

        // Rating Distribution
        var distribution = new Dictionary<int, int>
        {
            [5] = reviews.Count(r => r.Rating == 5),
            [4] = reviews.Count(r => r.Rating == 4),
            [3] = reviews.Count(r => r.Rating == 3),
            [2] = reviews.Count(r => r.Rating == 2),
            [1] = reviews.Count(r => r.Rating == 1)
        };

        return new InstructorReviewsDto
        {
            TotalReviews = totalReviews,
            AverageRating = averageRating,
            RatingDistribution = distribution,
            Reviews = reviews.Select(r => new InstructorReviewItemDto
            {
                Id = r.Id,
                CourseId = r.CourseId,
                CourseTitle = r.Course.Title,
                StudentName = r.Student.FullName,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            }).ToList()
        };
    }
}