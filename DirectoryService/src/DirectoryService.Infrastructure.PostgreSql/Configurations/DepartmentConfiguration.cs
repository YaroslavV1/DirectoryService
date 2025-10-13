using DirectoryService.Domain.Modules.DepartmentEntity;
using DirectoryService.Domain.Modules.DepartmentEntity.ValueObjects;
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
            .HasColumnName("parent_id");

        builder.HasOne(d => d.Parent)
            .WithMany(p => p.ChildDepartments)
            .IsRequired(false)
            .HasForeignKey(d => d.ParentId)
            .OnDelete(DeleteBehavior.ClientNoAction);

        builder.Property(d => d.ParentId)
            .IsRequired(false)
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

        builder.Property(d => d.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(d => d.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

    }
}