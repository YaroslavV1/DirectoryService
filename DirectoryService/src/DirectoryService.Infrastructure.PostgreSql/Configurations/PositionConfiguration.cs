using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Positions.ValueObjects;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("positions");

        builder.HasKey(p => p.Id)
            .HasName("pk_position");

        builder.Property(p => p.Id)
            .HasConversion(
                p => p.Value,
                id => PositionId.Create(id));

        builder.ComplexProperty(p => p.Name, pb =>
        {
            pb.Property(pn => pn.Value)
                .IsRequired()
                .HasColumnName("name")
                .HasMaxLength(LengthConstants.MAX_POSITION_NAME);
        });

        builder.Property(p => p.Description)
            .IsRequired()
            .HasColumnName("description")
            .HasMaxLength(LengthConstants.MAX_POSITION_DESCRIPTION);

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasColumnName("is_active");
        
        builder.HasMany(p => p.Departments)
            .WithOne()
            .HasForeignKey(d => d.PositionId);
    }
}