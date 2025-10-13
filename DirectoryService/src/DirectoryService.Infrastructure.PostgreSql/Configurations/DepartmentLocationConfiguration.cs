using DirectoryService.Domain.Modules.DepartmentEntity;
using DirectoryService.Domain.Modules.DepartmentEntity.ValueObjects;
using DirectoryService.Domain.Modules.LocationEntity.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentLocationConfiguration : IEntityTypeConfiguration<DepartmentLocation>
{
    public void Configure(EntityTypeBuilder<DepartmentLocation> builder)
    {
        builder.ToTable("department_locations");

        builder.HasKey(dp => new { dp.DepartmentId, dp.LocationId })
            .HasName("pk_department_locations");

        builder.Property(dl => dl.DepartmentId)
            .HasColumnName("department_id")
            .HasConversion(
                d => d.Value,
                id => DepartmentId.Create(id));

        builder.Property(dl => dl.LocationId)
            .HasColumnName("location_id")
            .HasConversion(
                l => l.Value,
                id => LocationId.Create(id));

        builder.HasOne(d => d.Department)
            .WithMany(dl => dl.Locations)
            .HasForeignKey(d => d.DepartmentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Location)
            .WithMany(l => l.Departments)
            .HasForeignKey(d => d.LocationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}