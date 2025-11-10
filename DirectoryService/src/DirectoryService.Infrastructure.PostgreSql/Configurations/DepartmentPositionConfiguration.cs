using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.DepartmentPositions.ValueObjects;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Positions.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentPositionConfiguration : IEntityTypeConfiguration<DepartmentPosition>
{
    public void Configure(EntityTypeBuilder<DepartmentPosition> builder)
    {
        builder.ToTable("department_positions");

        builder.HasKey(dp => dp.Id);

        builder.Property(dp => dp.Id).
            IsRequired()
            .HasColumnName("id")
            .HasConversion(
                value => value.Value,
                value => DepartmentPositionId.Create(value));

        builder.Property(dl => dl.DepartmentId)
            .HasColumnName("department_id")
            .HasConversion(
                d => d.Value,
                id => DepartmentId.Create(id));

        builder.Property(dl => dl.PositionId)
            .HasColumnName("position_id")
            .HasConversion(
                p => p.Value,
                id => PositionId.Create(id));

    }
}