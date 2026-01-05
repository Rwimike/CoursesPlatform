namespace CoursePlatform.Core.Entities;

public class Course
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public CourseStatus Status { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Relación con Lessons
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    
    // Métodos de negocio
    public bool CanBePublished()
    {
        return Lessons.Any(l => !l.IsDeleted);
    }
    
    public void Publish()
    {
        if (!CanBePublished())
            throw new InvalidOperationException("Cannot publish a course without active lessons");
        
        Status = CourseStatus.Published;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Unpublish()
    {
        Status = CourseStatus.Draft;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum CourseStatus
{
    Draft = 0,
    Published = 1
}