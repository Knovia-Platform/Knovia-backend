using CoursePlatform.Application.Specifications;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Application.Features.Reviews.Specifications;

public class ReviewsForAdminSpec : BaseSpecification<Review>
{
    public ReviewsForAdminSpec(int? courseId = null, int? rating = null)
        : base(r =>
            (!courseId.HasValue || r.CourseId == courseId.Value) &&
            (!rating.HasValue || r.Rating == rating.Value))
    {
        AddInclude(r => r.Student);
        AddInclude(r => r.Course);
        AddOrderByDesc(r => r.CreatedAt);
        ApplyNoTracking();
    }
}