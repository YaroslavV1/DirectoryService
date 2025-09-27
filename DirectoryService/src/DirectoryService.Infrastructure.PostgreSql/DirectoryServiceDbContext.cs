using DirectoryService.Domain.Modules.DepartmentEntity;
using DirectoryService.Domain.Modules.LocationEntity;
using DirectoryService.Domain.Modules.PositionEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure;

public class DirectoryServiceDbContext : DbContext
{
    private readonly string _connectionString;

    public DbSet<Department> Departments { get; set; }

    public DbSet<Position> Positions { get; set; }

    public DbSet<Location> Locations { get; set; }

    public DbSet<DepartmentLocation> DepartmentLocations { get; set; }

    public DbSet<DepartmentPosition> DepartmentPositions { get; set; }

    public DirectoryServiceDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connectionString);

        optionsBuilder.EnableDetailedErrors();
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.UseLoggerFactory(CreateLoggerFactory());
    }

    private ILoggerFactory CreateLoggerFactory() =>
        LoggerFactory.Create(builder => { builder.AddConsole(); });

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DirectoryServiceDbContext).Assembly);
    }
}