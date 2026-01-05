using CoursePlatform.Core.Entities;
using CoursePlatform.Core.Interfaces;
using CoursePlatform.Core.Services;
using FluentAssertions;
using Moq;

namespace CoursePlatform.Tests;

public class CourseServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICourseRepository> _courseRepoMock;
    private readonly Mock<ILessonRepository> _lessonRepoMock;
    private readonly CourseService _courseService;
    private readonly LessonService _lessonService;

    public CourseServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _courseRepoMock = new Mock<ICourseRepository>();
        _lessonRepoMock = new Mock<ILessonRepository>();

        _unitOfWorkMock.Setup(u => u.Courses).Returns(_courseRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Lessons).Returns(_lessonRepoMock.Object);

        _courseService = new CourseService(_unitOfWorkMock.Object);
        _lessonService = new LessonService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task PublishCourse_WithLessons_ShouldSucceed()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var course = new Course
        {
            Id = courseId,
            Title = "Test Course",
            Status = CourseStatus.Draft,
            Lessons = new List<Lesson>
            {
                new Lesson { Id = Guid.NewGuid(), CourseId = courseId, Title = "Lesson 1", Order = 1, IsDeleted = false }
            }
        };

        _courseRepoMock.Setup(r => r.GetWithLessonsAsync(courseId))
            .ReturnsAsync(course);
        
        var result = await _courseService.PublishCourseAsync(courseId);
        
        result.Should().NotBeNull();
        result!.Status.Should().Be(CourseStatus.Published);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task PublishCourse_WithoutLessons_ShouldFail()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var course = new Course
        {
            Id = courseId,
            Title = "Empty Course",
            Status = CourseStatus.Draft,
            Lessons = new List<Lesson>()
        };

        _courseRepoMock.Setup(r => r.GetWithLessonsAsync(courseId))
            .ReturnsAsync(course);

        // Act
        Func<Task> act = async () => await _courseService.PublishCourseAsync(courseId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot publish a course without active lessons");
    }

    [Fact]
    public async Task CreateLesson_WithUniqueOrder_ShouldSucceed()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var title = "New Lesson";
        var order = 1;

        _lessonRepoMock.Setup(r => r.OrderExistsInCourseAsync(courseId, order, null))
            .ReturnsAsync(false);

        // Act
        var result = await _lessonService.CreateLessonAsync(courseId, title, order);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(title);
        result.Order.Should().Be(order);
        result.CourseId.Should().Be(courseId);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateLesson_WithDuplicateOrder_ShouldFail()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var title = "Duplicate Order Lesson";
        var order = 1;

        _lessonRepoMock.Setup(r => r.OrderExistsInCourseAsync(courseId, order, null))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _lessonService.CreateLessonAsync(courseId, title, order);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Order {order} already exists in this course");
    }

    [Fact]
    public async Task DeleteCourse_ShouldBeSoftDelete()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var course = new Course
        {
            Id = courseId,
            Title = "Course to Delete",
            IsDeleted = false
        };

        _courseRepoMock.Setup(r => r.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        // Act
        var result = await _courseService.DeleteCourseAsync(courseId);

        // Assert
        result.Should().BeTrue();
        course.IsDeleted.Should().BeTrue();
        _courseRepoMock.Verify(r => r.Update(It.Is<Course>(c => c.IsDeleted == true)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}