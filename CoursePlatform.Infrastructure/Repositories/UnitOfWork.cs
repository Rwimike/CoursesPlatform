using CoursePlatform.Core.Interfaces;
using CoursePlatform.Infrastructure.Data;

namespace CoursePlatform.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private ICourseRepository? _courses;
    private ILessonRepository? _lessons;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public ICourseRepository Courses
    {
        get
        {
            _courses ??= new CourseRepository(_context);
            return _courses;
        }
    }

    public ILessonRepository Lessons
    {
        get
        {
            _lessons ??= new LessonRepository(_context);
            return _lessons;
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}