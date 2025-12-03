using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");

        builder.HasKey(l => l.Id)
            .HasName("pk_location");

        builder.Property(l => l.Id)
            .HasConversion(
                li => li.Value,
                id => LocationId.Create(id));

        builder.OwnsOne(l => l.Name, lb =>
        {
            lb.Property(ln => ln.Value)
                .IsRequired()
                .HasColumnName("name")
                .HasMaxLength(LengthConstants.MAX_LOCATION_NAME);

            lb.HasIndex(ln => ln.Value)
                .IsUnique()
                .HasDatabaseName("ux_locations_name")
                .HasFilter("\"is_active\" IS TRUE");
        });

        builder.OwnsOne(l => l.Address, lb =>
        {
            lb.Property(l => l.City)
                .IsRequired()
                .HasColumnName("city");

            lb.Property(l => l.House)
                .IsRequired()
                .HasColumnName("house");

            lb.Property(l => l.PostalCode)
                .IsRequired()
                .HasColumnName("postal_code");

            lb.Property(l => l.Street)
                .IsRequired()
                .HasColumnName("street");

            lb.HasIndex(a =>
                    new { a.City,
                        a.Street,
                        a.House,
                        a.PostalCode, })
                .IsUnique()
                .HasDatabaseName("ux_locations_full_address")
                .HasFilter("\"is_active\" IS TRUE");
        });

        builder.ComplexProperty(l => l.TimeZone, lb =>
        {
            lb.Property(tm => tm.Value)
                .IsRequired()
                .HasColumnName("time_zone");
        });

        builder.Property(l => l.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(l => l.UpdatedAt)
            .IsRequired()
            .HasColumnName("updated_at");
        
        builder.Property(l => l.DeletedAt)
            .HasColumnName("deleted_at")
            .IsRequired();

        builder.Property(l => l.IsActive)
            .IsRequired()
            .HasColumnName("is_active");

        builder.HasMany(l => l.Departments)
            .WithOne()
            .HasForeignKey(d => d.LocationId);
    }
}