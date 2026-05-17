using CoursePlatform.Application.Specifications;
using CoursePlatform.Domain.Entities;

public class ReviewsByInstructorSpec : BaseSpecification<Review>
{
    public ReviewsByInstructorSpec(Guid instructorId, int? courseId = null)
        : base(r =>
            r.Course.InstructorId == instructorId &&  
            (!courseId.HasValue || r.CourseId == courseId.Value))
    {
        AddInclude(r => r.Student);
        AddInclude(r => r.Course);
        AddOrderByDesc(r => r.CreatedAt);
        ApplyNoTracking();
    }
}