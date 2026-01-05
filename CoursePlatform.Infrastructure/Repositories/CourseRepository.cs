using CoursePlatform.Core.Entities;
using CoursePlatform.Core.Interfaces;
using CoursePlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoursePlatform.Infrastructure.Repositories;

public class CourseRepository : Repository<Course>, ICourseRepository
{
    public CourseRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<(IEnumerable<Course> Items, int TotalCount)> SearchAsync(
        string? searchTerm, 
        CourseStatus? status, 
        int page, 
        int pageSize)
    {
        var query = _dbSet.AsQueryable();

        // Filtro por búsqueda
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c => c.Title.Contains(searchTerm));
        }

        // Filtro por estado
        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Course?> GetWithLessonsAsync(Guid id)
    {
        return await _dbSet
            .Include(c => c.Lessons)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}