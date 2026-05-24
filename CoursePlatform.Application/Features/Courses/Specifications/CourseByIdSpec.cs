using CoursePlatform.Application.Specifications;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Application.Features.Courses.Specifications;

public class CourseByIdSpec : BaseSpecification<Course>
{
    public CourseByIdSpec(int id)
        : base(c => c.Id == id)
    {
        AddInclude(c => c.Instructor);
        AddInclude(c => c.SubCategory);
        AddInclude(c => c.Enrollments);
        AddInclude("SubCategory.Category");
        ApplyNoTracking();
    }
}