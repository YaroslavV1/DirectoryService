using DirectoryService.Domain.Modules.DepartmentEntity;
using DirectoryService.Domain.Modules.DepartmentEntity.ValueObjects;
using DirectoryService.Domain.Modules.PositionEntity.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentPositionConfiguration : IEntityTypeConfiguration<DepartmentPosition>
{
    public void Configure(EntityTypeBuilder<DepartmentPosition> builder)
    {
        builder.ToTable("department_positions");

        builder.HasKey(dp => new { dp.DepartmentId, dp.PositionId })
            .HasName("pk_department_positions");

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

        builder.HasOne(d => d.Department)
            .WithMany(dp => dp.Positions)
            .HasForeignKey(d => d.DepartmentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Position)
            .WithMany(dp => dp.Departments)
            .HasForeignKey(d => d.PositionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}