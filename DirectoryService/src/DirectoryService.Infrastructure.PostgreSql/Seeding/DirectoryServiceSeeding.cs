using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentLocations.ValueObjects;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.DepartmentPositions.ValueObjects;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Positions.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Seeding;

public class DirectoryServiceSeeding : ISeeder
{
    private readonly DirectoryServiceDbContext _dbContext;
    private readonly ILogger<DirectoryServiceSeeding> _logger;
    private readonly Random _random;

    // Конфигурационные константы
    private const int LOCATIONS_COUNT = 10;
    private const int ROOT_DEPARTMENTS_COUNT = 3;
    private const int CHILD_DEPARTMENTS_PER_PARENT = 4;
    private const int MAX_DEPARTMENT_DEPTH = 4;
    private const int POSITIONS_COUNT = 15;
    private const int MIN_LOCATIONS_PER_DEPARTMENT = 1;
    private const int MAX_LOCATIONS_PER_DEPARTMENT = 3;
    private const int MIN_DEPARTMENTS_PER_POSITION = 1;
    private const int MAX_DEPARTMENTS_PER_POSITION = 4;

    // Шаблонные данные
    private static readonly string[] CITIES =
    {
        "New York", "Los Angeles", "Chicago", "Houston", "Phoenix",
        "Philadelphia", "San Antonio", "San Diego", "Dallas", "San Jose",
        "Austin", "Jacksonville", "Fort Worth", "Columbus", "Charlotte",
    };

    private static readonly string[] STREETS =
    {
        "Main Street", "Oak Avenue", "Maple Drive", "Cedar Lane", "Pine Road",
        "Elm Street", "Washington Boulevard", "Park Avenue", "Broadway", "Market Street",
        "Hill Road", "Lake Drive", "Forest Lane", "River Street", "Sunset Boulevard",
    };

    private static readonly string[] TIMEZONES =
    {
        "America/New_York", "America/Chicago", "America/Denver",
        "America/Los_Angeles", "America/Phoenix", "America/Anchorage",
        "Pacific/Honolulu", "America/Detroit", "America/Indianapolis",
    };

    private static readonly string[] DEPARTMENT_NAMES =
    {
        "Development", "Testing", "DevOps", "Analytics", "Support",
        "Sales", "Marketing", "HR", "Finance", "Security",
        "Accounting", "Legal", "Logistics", "Procurement", "Production",
    };

    private static readonly string[] DEPARTMENT_PREFIXES =
    {
        "Backend", "Frontend", "Mobile", "QA", "Infrastructure",
        "Data", "Machine-Learning", "Security", "Business", "Customer",
    };

    private static readonly string[] POSITION_NAMES =
    {
        "Developer", "Tester", "Analyst", "Manager", "Director",
        "Engineer", "Specialist", "Team-Lead", "Architect", "Consultant",
        "Coordinator", "Administrator", "Expert", "Principal", "Intern",
    };

    private static readonly string[] POSITION_LEVELS =
    {
        "Junior", "Middle", "Senior", "Lead", "Principal",
    };

    private static readonly string[] POSITION_DESCRIPTIONS =
    {
        "Development and maintenance of software solutions",
        "Testing and quality assurance of products",
        "Requirements analysis and solution design",
        "Project and team management",
        "Technical customer support",
        "Business process optimization",
        "Information security assurance",
        "System and service administration",
        "Solution architecture development",
        "Technical consulting and advisory",
    };

    public DirectoryServiceSeeding(
        DirectoryServiceDbContext dbContext,
        ILogger<DirectoryServiceSeeding> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _random = new Random();
    }

    public async Task SeedAsync()
    {
        _logger.LogInformation("Starting seeding data...");

        try
        {
            await SeedData();
            _logger.LogInformation("Seeding finished");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Seeding data failed!!!");
            throw;
        }
    }

    private async Task SeedData()
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            await ClearDatabaseAsync();
            _logger.LogInformation("Database cleared successfully");

            var locations = await SeedLocationsAsync();
            _logger.LogInformation("Seeded {Count} locations", locations.Count);

            var departments = await SeedDepartmentsAsync(locations);
            _logger.LogInformation("Seeded {Count} departments", departments.Count);

            var positions = await SeedPositionsAsync(departments);
            _logger.LogInformation("Seeded {Count} positions", positions.Count);

            await transaction.CommitAsync();
            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error during database seeding");
            throw;
        }
    }

    private async Task ClearDatabaseAsync()
    {
        await _dbContext.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE department_positions, department_locations, departments, positions, locations RESTART IDENTITY CASCADE");
    }

    private async Task<List<Location>> SeedLocationsAsync()
    {
        var locations = new List<Location>();
        var usedAddresses = new HashSet<string>();

        for (int i = 0; i < LOCATIONS_COUNT; i++)
        {
            string city, street, house, postalCode, addressKey;

            do
            {
                city = CITIES[_random.Next(CITIES.Length)];
                street = STREETS[_random.Next(STREETS.Length)];
                house = _random.Next(1, 200).ToString();
                postalCode = _random.Next(10000, 99999).ToString();
                addressKey = $"{city}|{street}|{house}|{postalCode}";
            }
            while (usedAddresses.Contains(addressKey));

            usedAddresses.Add(addressKey);

            var location = Location.Create(
                LocationId.NewId(),
                LocationName.Create($"Office {city} {i + 1}").Value,
                Address.Create(city, street, postalCode, house).Value,
                LocationTimeZone.Create(TIMEZONES[_random.Next(TIMEZONES.Length)]).Value);

            locations.Add(location);
        }

        await _dbContext.Locations.AddRangeAsync(locations);
        await _dbContext.SaveChangesAsync();

        return locations;
    }

    private async Task<List<Department>> SeedDepartmentsAsync(List<Location> locations)
    {
        var departments = new List<Department>();
        var usedIdentifiers = new HashSet<string>();

        // Создание корневых департаментов
        for (int i = 0; i < ROOT_DEPARTMENTS_COUNT; i++)
        {
            var department = CreateDepartment(
                null,
                locations,
                usedIdentifiers,
                0);

            departments.Add(department);

            // Рекурсивное создание дочерних департаментов
            CreateChildDepartments(
                department,
                locations,
                usedIdentifiers,
                departments,
                1);
        }

        await _dbContext.Departments.AddRangeAsync(departments);
        await _dbContext.SaveChangesAsync();

        return departments;
    }

    private void CreateChildDepartments(
        Department parent,
        List<Location> locations,
        HashSet<string> usedIdentifiers,
        List<Department> allDepartments,
        int currentDepth)
    {
        if (currentDepth >= MAX_DEPARTMENT_DEPTH)
            return;

        int childCount = _random.Next(1, CHILD_DEPARTMENTS_PER_PARENT + 1);

        for (int i = 0; i < childCount; i++)
        {
            var child = CreateDepartment(
                parent,
                locations,
                usedIdentifiers,
                currentDepth);

            allDepartments.Add(child);

            // Рекурсивно создаем детей для текущего департамента
            if (_random.NextDouble() > 0.3) // 70% шанс создать детей
            {
                CreateChildDepartments(
                    child,
                    locations,
                    usedIdentifiers,
                    allDepartments,
                    currentDepth + 1);
            }
        }
    }

    private Department CreateDepartment(
        Department? parent,
        List<Location> locations,
        HashSet<string> usedIdentifiers,
        int depth)
    {
        string identifier;
        string name;

        do
        {
            var prefix = DEPARTMENT_PREFIXES[_random.Next(DEPARTMENT_PREFIXES.Length)];
            var baseName = DEPARTMENT_NAMES[_random.Next(DEPARTMENT_NAMES.Length)];
            var suffix = GenerateRandomLetters(4);

            identifier = $"{prefix}-{baseName}-{suffix}"
                .ToLower()
                .Replace(" ", "-");

            name = $"{prefix} {baseName}";

            if (parent != null)
            {
                name += $" {GenerateRandomLetters(2)}";
            }
        }
        while (usedIdentifiers.Contains(identifier) || identifier.Length > 150 || identifier.Length < 3);

        usedIdentifiers.Add(identifier);

        var departmentLocationCount = _random.Next(
            MIN_LOCATIONS_PER_DEPARTMENT,
            Math.Min(MAX_LOCATIONS_PER_DEPARTMENT, locations.Count) + 1);

        var selectedLocations = locations
            .OrderBy(_ => _random.Next())
            .Take(departmentLocationCount)
            .ToList();

        var departmentId = DepartmentId.NewId();
        var departmentLocations = selectedLocations
            .Select(l => DepartmentLocation.Create(
                DepartmentLocationId.NewGuid(),
                departmentId,
                l.Id).Value)
            .ToList();

        var departmentName = DepartmentName.Create(name).Value;
        var identifierValue = Identifier.Create(identifier).Value;

        Department department;

        if (parent == null)
        {
            department = Department.CreateParent(
                departmentName,
                identifierValue,
                departmentLocations,
                departmentId).Value;
        }
        else
        {
            department = Department.CreateChild(
                departmentName,
                identifierValue,
                parent,
                departmentLocations,
                departmentId).Value;
        }

        return department;
    }

    private string GenerateRandomLetters(int length)
    {
        const string letters = "abcdefghijklmnopqrstuvwxyz";
        var result = new char[length];

        for (int i = 0; i < length; i++)
        {
            result[i] = letters[_random.Next(letters.Length)];
        }

        return new string(result);
    }

    private async Task<List<Position>> SeedPositionsAsync(List<Department> departments)
    {
        var positions = new List<Position>();
        var usedNames = new HashSet<string>();

        for (int i = 0; i < POSITIONS_COUNT; i++)
        {
            string name;

            do
            {
                var baseName = POSITION_NAMES[_random.Next(POSITION_NAMES.Length)];
                var level = POSITION_LEVELS[_random.Next(POSITION_LEVELS.Length)];
                name = $"{level} {baseName}";
            }
            while (usedNames.Contains(name));

            usedNames.Add(name);

            var departmentCount = _random.Next(
                MIN_DEPARTMENTS_PER_POSITION,
                Math.Min(MAX_DEPARTMENTS_PER_POSITION, departments.Count) + 1);

            var selectedDepartments = departments
                .OrderBy(_ => _random.Next())
                .Take(departmentCount)
                .ToList();

            var positionId = PositionId.NewId();
            var departmentPositions = selectedDepartments
                .Select(d => DepartmentPosition.Create(
                    DepartmentPositionId.NewGuid(),
                    d.Id,
                    positionId).Value)
                .ToList();

            var description = _random.NextDouble() > 0.3
                ? POSITION_DESCRIPTIONS[_random.Next(POSITION_DESCRIPTIONS.Length)]
                : null;

            var position = Position.Create(
                positionId,
                PositionName.Create(name).Value,
                description,
                departmentPositions).Value;

            positions.Add(position);
        }

        await _dbContext.Positions.AddRangeAsync(positions);
        await _dbContext.SaveChangesAsync();

        return positions;
    }
}