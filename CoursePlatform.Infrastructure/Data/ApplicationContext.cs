using CoursePlatform.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CoursePlatform.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Lesson> Lessons => Set<Lesson>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configuración de Course
        builder.Entity<Course>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Title).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Status).HasConversion<string>();
            
            // Filtro global para IsDeleted
            entity.HasQueryFilter(c => !c.IsDeleted);
            
            // Relación con Lessons
            entity.HasMany(c => c.Lessons)
                  .WithOne(l => l.Course)
                  .HasForeignKey(l => l.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuración de Lesson
        builder.Entity<Lesson>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Title).IsRequired().HasMaxLength(200);
            
            // Filtro global para IsDeleted
            entity.HasQueryFilter(l => !l.IsDeleted);
            
            // Índice único para Order dentro del mismo Course
            entity.HasIndex(l => new { l.CourseId, l.Order })
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false"); // Solo aplica para no eliminados
        });

        // Seed de usuario de prueba
        SeedUser(builder);
    }

    private void SeedUser(ModelBuilder builder)
    {
        var userId = Guid.NewGuid().ToString();
        var hasher = new PasswordHasher<IdentityUser>();

        var user = new IdentityUser
        {
            Id = userId,
            UserName = "test@courseplatform.com",
            NormalizedUserName = "TEST@COURSEPLATFORM.COM",
            Email = "test@courseplatform.com",
            NormalizedEmail = "TEST@COURSEPLATFORM.COM",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        user.PasswordHash = hasher.HashPassword(user, "Test123!");

        builder.Entity<IdentityUser>().HasData(user);
    }
}