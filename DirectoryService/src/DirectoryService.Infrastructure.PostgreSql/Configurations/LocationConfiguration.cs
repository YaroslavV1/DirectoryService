using DirectoryService.Domain.Modules.LocationEntity;
using DirectoryService.Domain.Modules.LocationEntity.ValueObjects;
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

        builder.ComplexProperty(l => l.Name, lb =>
        {
            lb.Property(ln => ln.Value)
                .IsRequired()
                .HasColumnName("name")
                .HasMaxLength(LengthConstants.MAX_LOCATION_NAME);
        });

        builder.ComplexProperty(l => l.Address, lb =>
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
    }
}