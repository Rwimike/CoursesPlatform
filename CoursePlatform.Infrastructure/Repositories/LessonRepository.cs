using CoursePlatform.Core.Entities;
using CoursePlatform.Core.Interfaces;
using CoursePlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoursePlatform.Infrastructure.Repositories;

public class LessonRepository : Repository<Lesson>, ILessonRepository
{
    public LessonRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Lesson>> GetByCourseIdAsync(Guid courseId)
    {
        return await _dbSet
            .Where(l => l.CourseId == courseId)
            .OrderBy(l => l.Order)
            .ToListAsync();
    }

    public async Task<bool> OrderExistsInCourseAsync(Guid courseId, int order, Guid? excludeLessonId = null)
    {
        var query = _dbSet.Where(l => l.CourseId == courseId && l.Order == order);

        if (excludeLessonId.HasValue)
        {
            query = query.Where(l => l.Id != excludeLessonId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<Lesson?> GetByOrderInCourseAsync(Guid courseId, int order)
    {
        return await _dbSet
            .FirstOrDefaultAsync(l => l.CourseId == courseId && l.Order == order);
    }
}