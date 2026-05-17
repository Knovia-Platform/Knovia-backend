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

namespace CoursePlatform.Application.Features.Courses.Commands.CreateCourse;

public class CreateCourseCommandHandler
    : IRequestHandler<CreateCourseCommand, CourseDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public CreateCourseCommandHandler(
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
        CreateCourseCommand request, CancellationToken ct)
    {
        var instructorId = _currentUser.UserId
            ?? throw new UnauthorizedException();

        var subCategory = await _uow.Repository<SubCategory>()
                                    .GetByIdAsync(request.SubCategoryId, ct)
            ?? throw new NotFoundException("SubCategory", request.SubCategoryId);

        var course = new Course
        {
            Title = request.Title,
            Description = request.Description,
            ShortDescription = request.ShortDescription,
            Price = request.Price,
            Level = request.Level,
            Language = request.Language,
            SubCategoryId = request.SubCategoryId,
            InstructorId = instructorId,
            Status = CourseStatus.Draft,
            ThumbnailUrl = request.ThumbnailUrl,  
            PreviewVideoUrl = request.PreviewVideoUrl,
            Requirements = JsonSerializer.Serialize(request.Requirements),
            WhatYouLearn = JsonSerializer.Serialize(request.WhatYouLearn),
        };

        await _uow.Repository<Course>().AddAsync(course, ct);
        await _uow.CompleteAsync(ct);

        //// invalidate published courses cache
        //await _cache.RemoveByPrefixAsync("courses:published:", ct);

        // re-fetch with full includes
        var spec = new CourseByIdSpec(course.Id);
        var result = await _uow.Repository<Course>()
                               .GetEntityWithSpecAsync(spec, ct);

        return _mapper.Map<CourseDto>(result!);
    }
}