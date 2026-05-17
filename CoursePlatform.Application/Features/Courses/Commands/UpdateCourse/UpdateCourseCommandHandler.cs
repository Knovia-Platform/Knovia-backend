// Application/Features/Courses/Commands/UpdateCourse/UpdateCourseCommandHandler.cs
using AutoMapper;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Contracts.Persistence;
using CoursePlatform.Application.Contracts.Services;
using CoursePlatform.Application.Features.Courses.DTOs;
using CoursePlatform.Application.Features.Courses.Specifications;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;
using MediatR;
using System.Text.Json;

namespace CoursePlatform.Application.Features.Courses.Commands.UpdateCourse;

public class UpdateCourseCommandHandler
    : IRequestHandler<UpdateCourseCommand, CourseDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public UpdateCourseCommandHandler(
        IUnitOfWork uow,
        IMapper mapper,
        ICurrentUserService currentUser,
        ICacheService cache)
    {
        _uow = uow;
        _mapper = mapper;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<CourseDto> Handle(
        UpdateCourseCommand request, CancellationToken ct)
    {
        var course = await _uow.Repository<Course>()
                               .GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("Course", request.Id);

        // Ownership check
        if (course.InstructorId != _currentUser.UserId)
            throw new ForbiddenException(
                "You do not have permission to edit this course.");

        // can't be update if UnderReview أو Archived
        if (course.Status is CourseStatus.UnderReview or CourseStatus.Archived)
            throw new BadRequestException(
                $"Cannot edit a course with status '{course.Status}'.");

        course.Title = request.Title;
        course.Description = request.Description;
        course.ShortDescription = request.ShortDescription;
        course.Price = request.Price;
        course.DiscountPrice = request.DiscountPrice;
        course.Level = request.Level;
        course.Language = request.Language;
        course.SubCategoryId = request.SubCategoryId;
        course.Requirements = JsonSerializer.Serialize(request.Requirements);
        course.WhatYouLearn = JsonSerializer.Serialize(request.WhatYouLearn);

        if (request.ThumbnailUrl is not null)
            course.ThumbnailUrl = request.ThumbnailUrl;

        if (request.PreviewVideoUrl is not null)
            course.PreviewVideoUrl = request.PreviewVideoUrl;

        // لو كان Rejected وعدّله — يرجع Draft
        if (course.Status == CourseStatus.Rejected)
        {
            course.Status = CourseStatus.Draft;
            course.RejectionReason = null;
        }

        _uow.Repository<Course>().Update(course);
        await _uow.CompleteAsync(ct);

        await _cache.RemoveByPrefixAsync("courses:published:", ct);
        await _cache.RemoveAsync($"courses:detail:{request.Id}", ct);

        var spec = new CourseByIdSpec(course.Id);
        var result = await _uow.Repository<Course>()
                               .GetEntityWithSpecAsync(spec, ct);

        return _mapper.Map<CourseDto>(result!);
    }
}