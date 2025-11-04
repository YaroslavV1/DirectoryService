using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");

        builder.HasKey(d => d.Id)
            .HasName("pk_department");

        builder.Property(d => d.Id)
            .IsRequired()
            .HasColumnName("id")
            .HasConversion(
                d => d.Value,
                id => DepartmentId.Create(id));

        builder.ComplexProperty(d => d.Name, dnb =>
        {
            dnb.Property(d => d.Value)
                .IsRequired()
                .HasMaxLength(LengthConstants.MAX_DEPARTMENT_NAME)
                .HasColumnName("name");

        });

        builder.ComplexProperty(d => d.Identifier, dnb =>
        {
            dnb.Property(d => d.Value)
                .IsRequired()
                .HasMaxLength(LengthConstants.MAX_DEPARTMENT_ID)
                .HasColumnName("identifier");
        });

        builder.Property(d => d.ParentId)
            .IsRequired(false)
            .HasColumnName("parent_id")
            .HasConversion(
                d => d!.Value,
                id => DepartmentId.Create(id));

        builder.ComplexProperty(d => d.Path, dnb =>
        {
            dnb.Property(d => d.Value)
                .IsRequired()
                .HasColumnName("path");
        });

        builder.Property(d => d.Depth)
            .IsRequired()
            .HasColumnName("depth");

        builder.Property(d => d.ChildrenCount)
            .IsRequired()
            .HasColumnName("children_count");

        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasColumnName("is_active");

        builder.Property(d => d.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(d => d.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasMany(d => d.ChildrenDepartments)
            .WithOne()
            .IsRequired(false)
            .HasForeignKey(d => d.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(d => d.Locations)
            .WithOne()
            .HasForeignKey(d => d.DepartmentId);

        builder.HasMany(d => d.Positions)
            .WithOne()
            .HasForeignKey(d => d.DepartmentId);

    }
}