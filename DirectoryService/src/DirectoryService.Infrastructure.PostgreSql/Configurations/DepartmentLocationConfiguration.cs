using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentLocations.ValueObjects;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentLocationConfiguration : IEntityTypeConfiguration<DepartmentLocation>
{
    public void Configure(EntityTypeBuilder<DepartmentLocation> builder)
    {
        builder.ToTable("department_locations");

        builder.HasKey(dl => dl.Id);

        builder.Property(dl => dl.Id)
            .IsRequired()
            .HasColumnName("id")
            .HasConversion(
                value => value.Value,
                value => DepartmentLocationId.Create(value));

        builder.Property(dl => dl.DepartmentId)
            .IsRequired()
            .HasColumnName("department_id")
            .HasConversion(
                d => d.Value,
                id => DepartmentId.Create(id));

        builder.Property(dl => dl.LocationId)
            .IsRequired()
            .HasColumnName("location_id")
            .HasConversion(
                l => l.Value,
                id => LocationId.Create(id));
    }
}