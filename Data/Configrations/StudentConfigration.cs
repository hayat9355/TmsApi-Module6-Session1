using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

namespace TmsApi.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        // 1. Primary Key Configuration
        builder.HasKey(s => s.Id);

        // 2. Property Limits & Validation
        builder.Property(s => s.RegistrationNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.GPA)
            .HasPrecision(3, 2); // Prevents storage drift, maps to numeric(3,2) (e.g., 4.00)

        // 3. Natural Key Uniqueness Constraint
        builder.HasIndex(s => s.RegistrationNumber)
            .IsUnique();
    }
}
