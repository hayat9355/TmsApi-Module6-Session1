using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;
using Tms.Api.Dtos;

namespace TmsApi.Services;

public class EnrollmentService(
    TmsDbContext context,
    ILogger<EnrollmentService> logger)
    : IEnrollmentService
{

    public Task<EnrollmentResponseDto?> GetByIdAsync(
        int courseId,
        int id,
        CancellationToken ct) =>
        context.Enrollments
            .AsNoTracking()
            .Where(e => e.Id == id && e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.CourseId,
                e.StudentId,
                e.EnrolledAt))
            .FirstOrDefaultAsync(ct);

    public async Task<EnrollmentResponseDto> CreateAsync(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct)
    {
        var enrollment = new Enrollment
        {
            CourseId = courseId,
            StudentId = request.StudentId,
            EnrolledAt = DateTime.UtcNow
        };

        context.Enrollments.Add(enrollment);

        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Student {StudentId} enrolled in course {CourseId}",
            enrollment.StudentId,
            enrollment.CourseId);

        var result = await GetByIdAsync(
            courseId,
            enrollment.Id,
            ct);

        return result!;
    }

}
public class TmsDatabaseException(string message) : Exception(message);