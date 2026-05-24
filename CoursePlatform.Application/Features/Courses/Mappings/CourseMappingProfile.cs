using AutoMapper;
using CoursePlatform.Application.Features.Courses.DTOs;
using CoursePlatform.Domain.Entities;
using System.Text.Json;

namespace CoursePlatform.Application.Features.Courses.Mappings;

public class CourseMappingProfile : Profile
{
    public CourseMappingProfile()
    {
        CreateMap<Course, CourseSummaryDto>()
            .ForMember(d => d.Level,
                o => o.MapFrom(s => s.Level.ToString()))
            .ForMember(d => d.Status,
                o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.InstructorName,
                o => o.MapFrom(s => s.Instructor != null
                    ? s.Instructor.FullName : string.Empty))
            .ForMember(d => d.SubCategoryName,
                o => o.MapFrom(s => s.SubCategory != null
                    ? s.SubCategory.Name : string.Empty))
             .ForMember(d => d.AverageRating,
                o => o.MapFrom(s => s.AverageRating))   
            .ForMember(d => d.TotalStudents,
                o => o.MapFrom(s => s.Enrollments.Count)); 

        CreateMap<Course, CourseDto>()
            .ForMember(d => d.Level,
                o => o.MapFrom(s => s.Level.ToString()))
            .ForMember(d => d.Status,
                o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.InstructorName,
                o => o.MapFrom(s => s.Instructor != null
                    ? s.Instructor.FullName : string.Empty))
            .ForMember(d => d.SubCategoryName,
                o => o.MapFrom(s => s.SubCategory != null
                    ? s.SubCategory.Name : string.Empty))
            .ForMember(d => d.CategoryName,
                o => o.MapFrom(s => s.SubCategory != null
                    ? s.SubCategory.Category.Name : string.Empty))
            .ForMember(d => d.Requirements,
                o => o.MapFrom(s => DeserializeList(s.Requirements)))
            .ForMember(d => d.WhatYouLearn,
                o => o.MapFrom(s => DeserializeList(s.WhatYouLearn)))
            .ForMember(d => d.AverageRating,
                o => o.MapFrom(s => s.AverageRating))   
            .ForMember(d => d.TotalStudents,
                o => o.MapFrom(s => s.Enrollments.Count));
    }

    private static List<string> DeserializeList(string? json)
    {
        if (string.IsNullOrEmpty(json)) return [];
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }
}