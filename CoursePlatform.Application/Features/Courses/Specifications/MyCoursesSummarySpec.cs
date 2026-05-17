using CoursePlatform.Application.Specifications;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Application.Features.Courses.Specifications;

public class MyCoursesSummarySpec : BaseSpecification<Course>
{
    public MyCoursesSummarySpec(Guid instructorId)
        : base(c => c.InstructorId == instructorId)
    {
        AddOrderBy(c => c.Title);
        ApplyNoTracking();
    }
}