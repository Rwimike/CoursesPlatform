using CoursePlatform.Core.Entities;
using CoursePlatform.Core.Interfaces;

namespace CoursePlatform.Core.Services;

public interface ICourseService
{
    Task<(IEnumerable<Course> Items, int TotalCount)> SearchCoursesAsync(
        string? searchTerm, CourseStatus? status, int page, int pageSize);
    Task<Course?> GetCourseByIdAsync(Guid id);
    Task<CourseSummary?> GetCourseSummaryAsync(Guid id);
    Task<Course> CreateCourseAsync(string title);
    Task<Course?> UpdateCourseAsync(Guid id, string title);
    Task<bool> DeleteCourseAsync(Guid id);
    Task<Course?> PublishCourseAsync(Guid id);
    Task<Course?> UnpublishCourseAsync(Guid id);
}

public class CourseService : ICourseService
{
    private readonly IUnitOfWork _unitOfWork;

    public CourseService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<(IEnumerable<Course> Items, int TotalCount)> SearchCoursesAsync(
        string? searchTerm, CourseStatus? status, int page, int pageSize)
    {
        return await _unitOfWork.Courses.SearchAsync(searchTerm, status, page, pageSize);
    }

    public async Task<Course?> GetCourseByIdAsync(Guid id)
    {
        return await _unitOfWork.Courses.GetByIdAsync(id);
    }

    public async Task<CourseSummary?> GetCourseSummaryAsync(Guid id)
    {
        var course = await _unitOfWork.Courses.GetWithLessonsAsync(id);
        if (course == null) return null;

        return new CourseSummary
        {
            Id = course.Id,
            Title = course.Title,
            Status = course.Status,
            TotalLessons = course.Lessons.Count(l => !l.IsDeleted),
            LastModified = course.UpdatedAt
        };
    }

    public async Task<Course> CreateCourseAsync(string title)
    {
        var course = new Course
        {
            Id = Guid.NewGuid(),
            Title = title,
            Status = CourseStatus.Draft,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Courses.AddAsync(course);
        await _unitOfWork.SaveChangesAsync();

        return course;
    }

    public async Task<Course?> UpdateCourseAsync(Guid id, string title)
    {
        var course = await _unitOfWork.Courses.GetByIdAsync(id);
        if (course == null) return null;

        course.Title = title;
        course.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Courses.Update(course);
        await _unitOfWork.SaveChangesAsync();

        return course;
    }

    public async Task<bool> DeleteCourseAsync(Guid id)
    {
        var course = await _unitOfWork.Courses.GetByIdAsync(id);
        if (course == null) return false;

        course.SoftDelete();
        _unitOfWork.Courses.Update(course);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<Course?> PublishCourseAsync(Guid id)
    {
        var course = await _unitOfWork.Courses.GetWithLessonsAsync(id);
        if (course == null) return null;

        course.Publish(); // Valida que tenga lecciones activas
        _unitOfWork.Courses.Update(course);
        await _unitOfWork.SaveChangesAsync();

        return course;
    }

    public async Task<Course?> UnpublishCourseAsync(Guid id)
    {
        var course = await _unitOfWork.Courses.GetByIdAsync(id);
        if (course == null) return null;

        course.Unpublish();
        _unitOfWork.Courses.Update(course);
        await _unitOfWork.SaveChangesAsync();

        return course;
    }
}

public class CourseSummary
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public CourseStatus Status { get; set; }
    public int TotalLessons { get; set; }
    public DateTime LastModified { get; set; }
}