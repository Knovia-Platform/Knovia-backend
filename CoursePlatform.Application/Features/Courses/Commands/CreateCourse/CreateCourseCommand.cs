using CoursePlatform.Application.Features.Courses.DTOs;
using CoursePlatform.Domain.Enums;
using MediatR;

namespace CoursePlatform.Application.Features.Courses.Commands.CreateCourse;

public record CreateCourseCommand(
    string Title,
    string Description,
    string? ShortDescription,
    decimal Price,
    CourseLevel Level,
    string Language,
    int SubCategoryId,
    List<string> Requirements,
    List<string> WhatYouLearn
    ,string? ThumbnailUrl,
    string? PreviewVideoUrl
) : IRequest<CourseDto>;