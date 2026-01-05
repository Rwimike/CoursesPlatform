namespace CoursePlatform.Core.Entities;

public class Lesson
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Relación con Course
    public Course Course { get; set; } = null!;
    
    // Métodos de negocio
    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void MoveUp()
    {
        if (Order > 1)
        {
            Order--;
            UpdatedAt = DateTime.UtcNow;
        }
    }
    
    public void MoveDown()
    {
        Order++;
        UpdatedAt = DateTime.UtcNow;
    }
}
