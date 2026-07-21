using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Entities;

namespace TmsApi.Services;

public class CourseService(
    TmsDbContext context,
    ILogger<CourseService> logger) : ICourseService
// ↑ Primary constructor DI — cleaner than a constructor body
// TmsDbContext → talks to PostgreSQL
// ILogger → logs important events (creations, errors)
{
    public Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct) =>
        context.Courses
            .AsNoTracking()
            // ↑ Read-only query — no need to track changes
            // Saves memory and CPU — EF won't watch this object for changes
            .Where(c => c.Id == id)
            .Select(c => new CourseResponseDto(
                c.Id,
                c.Code,
                c.Title,
                c.MaxCapacity,
                c.Enrollments.Count))
            // ↑ c.Enrollments.Count → EF translates to SQL COUNT subquery
            // We never load enrollment objects into memory
            // SQL: SELECT c.Id, c.Code, c.Title, c.MaxCapacity,
            //      (SELECT COUNT(*) FROM Enrollments WHERE CourseId = c.Id)
            //      FROM Courses WHERE c.Id = @id
            .FirstOrDefaultAsync(ct);
    // ↑ Returns null if not found — ICourseService contract says CourseResponseDto?

    public async Task<CourseResponseDto> CreateAsync(
        CreateCourseRequest request, CancellationToken ct)
    {
        // Map DTO → Entity (only service knows about entities, not controller)
        var course = new Course
        {
            Code = request.Code,
            Title = request.Title,
            MaxCapacity = request.MaxCapacity
        };

        context.Courses.Add(course);
        // ↑ Tells EF Core: "track this new course, mark it as Added"
        // No SQL runs yet

        await context.SaveChangesAsync(ct);
        // ↑ NOW the SQL runs:
        // INSERT INTO "Courses" ("Code", "Title", "MaxCapacity")
        // VALUES ('CSE-101', 'Web Dev', 30)
        // PostgreSQL sets the Id automatically (IDENTITY column)

        logger.LogInformation(
            "Created course {CourseId} ({Code})", course.Id, course.Code);
        // ↑ One log line per write operation — production best practice
        // Never log on every read — too noisy

        // Re-query through GetByIdAsync so response uses the same projection
        // This guarantees the response DTO is always consistent
        return (await GetByIdAsync(course.Id, ct))!;
        // ↑ The ! says "I know this won't be null — I just inserted it"
    }

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        context.Courses
            .AsNoTracking()
            .AnyAsync(c => c.Code == code, ct);
    // ↑ AnyAsync → SQL: SELECT EXISTS (SELECT 1 FROM "Courses" WHERE "Code" = 'CSE-101' LIMIT 1)
    // Stops at the FIRST matching row — very fast
    // Returns true if exists, false if not
}