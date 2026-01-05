using CoursePlatform.Core.Entities;
using CoursePlatform.Core.Interfaces;

namespace CoursePlatform.Core.Services;

public interface ILessonService
{
    Task<IEnumerable<Lesson>> GetLessonsByCourseAsync(Guid courseId);
    Task<Lesson?> GetLessonByIdAsync(Guid id);
    Task<Lesson> CreateLessonAsync(Guid courseId, string title, int order);
    Task<Lesson?> UpdateLessonAsync(Guid id, string title, int order);
    Task<bool> DeleteLessonAsync(Guid id);
    Task<bool> ReorderLessonAsync(Guid id, bool moveUp);
}

public class LessonService : ILessonService
{
    private readonly IUnitOfWork _unitOfWork;

    public LessonService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Lesson>> GetLessonsByCourseAsync(Guid courseId)
    {
        var lessons = await _unitOfWork.Lessons.GetByCourseIdAsync(courseId);
        return lessons.OrderBy(l => l.Order);
    }

    public async Task<Lesson?> GetLessonByIdAsync(Guid id)
    {
        return await _unitOfWork.Lessons.GetByIdAsync(id);
    }

    public async Task<Lesson> CreateLessonAsync(Guid courseId, string title, int order)
    {

        var orderExists = await _unitOfWork.Lessons.OrderExistsInCourseAsync(courseId, order);
        if (orderExists)
        {
            throw new InvalidOperationException($"Order {order} already exists in this course");
        }

        var lesson = new Lesson
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Title = title,
            Order = order,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Lessons.AddAsync(lesson);
        await _unitOfWork.SaveChangesAsync();

        return lesson;
    }

    public async Task<Lesson?> UpdateLessonAsync(Guid id, string title, int order)
    {
        var lesson = await _unitOfWork.Lessons.GetByIdAsync(id);
        if (lesson == null) return null;


        var orderExists = await _unitOfWork.Lessons.OrderExistsInCourseAsync(
            lesson.CourseId, order, id);
        
        if (orderExists)
        {
            throw new InvalidOperationException($"Order {order} already exists in this course");
        }

        lesson.Title = title;
        lesson.Order = order;
        lesson.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Lessons.Update(lesson);
        await _unitOfWork.SaveChangesAsync();

        return lesson;
    }

    public async Task<bool> DeleteLessonAsync(Guid id)
    {
        var lesson = await _unitOfWork.Lessons.GetByIdAsync(id);
        if (lesson == null) return false;

        lesson.SoftDelete();
        _unitOfWork.Lessons.Update(lesson);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ReorderLessonAsync(Guid id, bool moveUp)
    {
        var lesson = await _unitOfWork.Lessons.GetByIdAsync(id);
        if (lesson == null) return false;

        var targetOrder = moveUp ? lesson.Order - 1 : lesson.Order + 1;
        
        var targetLesson = await _unitOfWork.Lessons.GetByOrderInCourseAsync(
            lesson.CourseId, targetOrder);

        if (targetLesson == null) return false;
        
        var tempOrder = lesson.Order;
        lesson.Order = targetLesson.Order;
        targetLesson.Order = tempOrder;

        lesson.UpdatedAt = DateTime.UtcNow;
        targetLesson.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Lessons.Update(lesson);
        _unitOfWork.Lessons.Update(targetLesson);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}