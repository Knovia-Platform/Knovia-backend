using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Contracts.Persistence;
using CoursePlatform.Application.Contracts.Services;
using CoursePlatform.Application.Features.Courses.Specifications;
using CoursePlatform.Domain.Entities;
using MediatR;

namespace CoursePlatform.Application.Features.Courses.Queries.GetMyCourses;

public class GetMyCoursesForFilterQueryHandler
    : IRequestHandler<GetMyCoursesForFilterQuery,
                      IReadOnlyList<CourseFilterItemDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetMyCoursesForFilterQueryHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<CourseFilterItemDto>> Handle(
        GetMyCoursesForFilterQuery request, CancellationToken ct)
    {
        var instructorId = _currentUser.UserId
            ?? throw new UnauthorizedException();

        var spec = new MyCoursesSummarySpec(instructorId);
        var courses = await _uow.Repository<Course>()
                                .GetAllWithSpecAsync(spec, ct);

        return courses
            .Select(c => new CourseFilterItemDto
            {
                Id = c.Id,
                Title = c.Title
            })
            .ToList();
    }
}