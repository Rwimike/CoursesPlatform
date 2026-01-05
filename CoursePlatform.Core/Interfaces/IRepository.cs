using CoursePlatform.Core.Entities;

namespace CoursePlatform.Core.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}

public interface ICourseRepository : IRepository<Course>
{
    Task<(IEnumerable<Course> Items, int TotalCount)> SearchAsync(
        string? searchTerm, 
        CourseStatus? status, 
        int page, 
        int pageSize);
    
    Task<Course?> GetWithLessonsAsync(Guid id);
}

public interface ILessonRepository : IRepository<Lesson>
{
    Task<IEnumerable<Lesson>> GetByCourseIdAsync(Guid courseId);
    Task<bool> OrderExistsInCourseAsync(Guid courseId, int order, Guid? excludeLessonId = null);
    Task<Lesson?> GetByOrderInCourseAsync(Guid courseId, int order);
}

public interface IUnitOfWork : IDisposable
{
    ICourseRepository Courses { get; }
    ILessonRepository Lessons { get; }
    Task<int> SaveChangesAsync();
}