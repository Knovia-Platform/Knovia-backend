using CoursePlatform.Application.Features.Reviews.DTOs;
using MediatR;

namespace CoursePlatform.Application.Features.Reviews.Queries.GetInstructorReviews;

public record GetInstructorReviewsQuery(
    int? CourseId = null   // null = كل الـ courses
) : IRequest<InstructorReviewsDto>;