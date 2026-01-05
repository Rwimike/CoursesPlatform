using CoursePlatform.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoursePlatform.API.Controllers;

[Authorize]
[ApiController]
[Route("api/courses/{courseId}/[controller]")]
public class LessonsController : ControllerBase
{
    private readonly ILessonService _lessonService;

    public LessonsController(ILessonService lessonService)
    {
        _lessonService = lessonService;
    }

    [HttpGet]
    public async Task<IActionResult> GetByCourse(Guid courseId)
    {
        var lessons = await _lessonService.GetLessonsByCourseAsync(courseId);
        return Ok(lessons);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid courseId, Guid id)
    {
        var lesson = await _lessonService.GetLessonByIdAsync(id);

        if (lesson == null || lesson.CourseId != courseId)
            return NotFound();

        return Ok(lesson);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid courseId, [FromBody] CreateLessonRequest request)
    {
        try
        {
            var lesson = await _lessonService.CreateLessonAsync(courseId, request.Title, request.Order);
            return CreatedAtAction(nameof(GetById), new { courseId, id = lesson.Id }, lesson);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid courseId, Guid id, [FromBody] UpdateLessonRequest request)
    {
        try
        {
            var lesson = await _lessonService.UpdateLessonAsync(id, request.Title, request.Order);

            if (lesson == null || lesson.CourseId != courseId)
                return NotFound();

            return Ok(lesson);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid courseId, Guid id)
    {
        var lesson = await _lessonService.GetLessonByIdAsync(id);

        if (lesson == null || lesson.CourseId != courseId)
            return NotFound();

        var result = await _lessonService.DeleteLessonAsync(id);

        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpPatch("{id}/reorder")]
    public async Task<IActionResult> Reorder(Guid courseId, Guid id, [FromBody] ReorderLessonRequest request)
    {
        var lesson = await _lessonService.GetLessonByIdAsync(id);

        if (lesson == null || lesson.CourseId != courseId)
            return NotFound();

        var result = await _lessonService.ReorderLessonAsync(id, request.MoveUp);

        if (!result)
            return BadRequest(new { message = "Cannot reorder lesson" });

        return Ok();
    }
}

public record CreateLessonRequest(string Title, int Order);
public record UpdateLessonRequest(string Title, int Order);
public record ReorderLessonRequest(bool MoveUp);