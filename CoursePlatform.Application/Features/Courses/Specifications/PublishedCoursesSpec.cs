using CoursePlatform.Application.Features.Courses.DTOs;
using CoursePlatform.Application.Specifications;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.Courses.Specifications;

public class PublishedCoursesSpec : BaseSpecification<Course>
{
    public PublishedCoursesSpec(CourseQueryParams p)
        : base(c =>
            c.Status == CourseStatus.Published &&
            (string.IsNullOrEmpty(p.Search) ||
                c.Title.ToLower().Contains(p.Search.ToLower()) ||
                c.Description.ToLower().Contains(p.Search.ToLower())) &&
            (!p.SubCategoryId.HasValue || c.SubCategoryId == p.SubCategoryId) &&
            (!p.CategoryId.HasValue ||
                c.SubCategory.Category.Id == p.CategoryId) &&
            (!p.Level.HasValue || c.Level == p.Level) &&
            (string.IsNullOrEmpty(p.Language) ||
                c.Language.ToLower() == p.Language.ToLower()) &&
            (!p.MinPrice.HasValue || c.Price >= p.MinPrice) &&
            (!p.MaxPrice.HasValue || c.Price <= p.MaxPrice)
        )
    {
        AddInclude(c => c.Instructor);
        AddInclude(c => c.SubCategory);
        AddInclude(c => c.Enrollments);
        AddInclude("SubCategory.Category");

        switch (p.SortBy)
        {
            case "price_asc": AddOrderBy(c => c.Price); break;
            case "price_desc": AddOrderByDesc(c => c.Price); break;
            default: AddOrderByDesc(c => c.CreatedAt); break;
        }

        ApplyPaging(p.PageIndex, p.PageSize);
        ApplyNoTracking();
    }
}