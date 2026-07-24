using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

namespace TmsApi.Data.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        // 1. Primary Key
        builder.HasKey(e => e.Id);

        // 2. Grade precision
        builder.Property(e => e.Grade)
            .HasPrecision(3, 2);


        // EXERCISE 5 TODOs: RELATIONSHIPS AND DELETE BEHAVIORS
        

        // Relationship A: One Student has many Enrollments.
        // Deleting a student profile cascade-deletes their current active registrations.
        builder.HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship B: One Course has many Enrollments.
        // Rule: Prevent deleting a Course if students are currently registered in it.
        // This forces administrators to manually handle active registrants first, preventing orphan grade records.
        builder.HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict); // 👈 Strict deletion restriction
    }
}
