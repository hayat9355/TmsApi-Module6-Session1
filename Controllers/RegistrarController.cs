using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TmsApi.Data;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/registrar")]
public class RegistrarController(TmsDbContext context) : ControllerBase
{
    // TODO 1: Paginated list of students with stable sort
    [HttpGet("students/paged")]
    public async Task<IActionResult> GetPagedStudents(
        [FromQuery] int page = 1, 
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        const int pageSize = 20;

        var students = await context.Students
            .OrderBy(s => s.Name) // Always OrderBy before Skip/Take for a stable sort
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Ok(students);
    }

    // TODO 2: Top 5 courses by enrollment count
    [HttpGet("courses/top")]
    public async Task<IActionResult> GetTopCourses(CancellationToken cancellationToken = default)
    {
        var topCourses = await context.Enrollments
            .GroupBy(e => e.Course.Title)
            .Select(g => new
            {
                CourseTitle = g.Key,
                EnrollmentCount = g.Count()
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .Take(5)
            .ToListAsync(cancellationToken);

        return Ok(topCourses);
    }
}
