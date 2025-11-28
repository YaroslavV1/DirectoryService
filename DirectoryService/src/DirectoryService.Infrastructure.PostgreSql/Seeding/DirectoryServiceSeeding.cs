using System.Reflection;
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

    // Конфигурационные константы - УВЕЛИЧЕНЫ ДЛЯ ТЕСТОВ
    private const int LOCATIONS_COUNT = 100;
    private const int ROOT_DEPARTMENTS_COUNT = 15;
    private const int CHILD_DEPARTMENTS_PER_PARENT = 5;
    private const int MAX_DEPARTMENT_DEPTH = 4;
    private const int POSITIONS_COUNT = 80;
    private const int MIN_LOCATIONS_PER_DEPARTMENT = 1;
    private const int MAX_LOCATIONS_PER_DEPARTMENT = 4;
    private const int MIN_DEPARTMENTS_PER_POSITION = 2;
    private const int MAX_DEPARTMENTS_PER_POSITION = 6;

    // Диапазон дат для CreatedAt/UpdatedAt (последние 2 года)
    private static readonly DateTime START_DATE = DateTime.UtcNow.AddYears(-2);
    private static readonly DateTime END_DATE = DateTime.UtcNow;

    // РАСШИРЕННЫЕ шаблонные данные для разнообразия
    private static readonly string[] CITIES =
    [
        "New York", "Los Angeles", "Chicago", "Houston", "Phoenix", "Philadelphia", "San Antonio", "San Diego",
        "Dallas", "San Jose", "Austin", "Jacksonville", "Fort Worth", "Columbus", "Charlotte", "San Francisco",
        "Indianapolis", "Seattle", "Denver", "Washington", "Boston", "Nashville", "Detroit", "Portland",
        "Las Vegas", "Memphis", "Louisville", "Baltimore", "Milwaukee", "Albuquerque", "Tucson", "Fresno",
        "Sacramento", "Kansas City", "Mesa", "Atlanta", "Omaha", "Colorado Springs", "Raleigh", "Miami"
    ];

    private static readonly string[] STREETS =
    [
        "Main Street", "Oak Avenue", "Maple Drive", "Cedar Lane", "Pine Road", "Elm Street", "Washington Boulevard",
        "Park Avenue", "Broadway", "Market Street", "Hill Road", "Lake Drive", "Forest Lane", "River Street",
        "Sunset Boulevard", "Spring Street", "Church Road", "Willow Avenue", "Cherry Lane", "Birch Drive",
        "Valley Road", "Mountain View", "Highland Avenue", "Meadow Lane", "Grove Street", "Woodland Drive",
        "Summit Avenue", "Ridge Road", "Lakeview Drive", "Hillside Avenue"
    ];

    private static readonly string[] TIMEZONES =
    [
        "America/New_York", "America/Chicago", "America/Denver", "America/Los_Angeles", "America/Phoenix",
        "America/Anchorage", "Pacific/Honolulu", "America/Detroit", "America/Indianapolis",
        "America/Kentucky/Louisville", "America/Boise", "America/Juneau"
    ];

    private static readonly string[] DEPARTMENT_NAMES =
    [
        "Development", "Testing", "DevOps", "Analytics", "Support", "Sales", "Marketing", "HR", "Finance",
        "Security", "Accounting", "Legal", "Logistics", "Procurement", "Production", "Research", "Innovation",
        "Quality", "Compliance", "Operations", "Strategy", "Planning", "Design", "Engineering", "Architecture"
    ];

    private static readonly string[] DEPARTMENT_PREFIXES =
    [
        "Backend", "Frontend", "Mobile", "QA", "Infrastructure", "Data", "Machine-Learning", "Security", "Business",
        "Customer", "Cloud", "Platform", "Product", "Growth", "Revenue", "Enterprise", "Digital", "Core",
        "Advanced", "Strategic"
    ];

    private static readonly string[] POSITION_NAMES =
    [
        "Developer", "Tester", "Analyst", "Manager", "Director", "Engineer", "Specialist", "Team-Lead", "Architect",
        "Consultant", "Coordinator", "Administrator", "Expert", "Principal", "Intern", "Supervisor", "Officer",
        "Associate", "Strategist", "Designer", "Planner", "Researcher", "Scientist", "Technician", "Operator"
    ];

    private static readonly string[] POSITION_LEVELS =
    [
        "Junior", "Middle", "Senior", "Lead", "Principal", "Staff", "Distinguished"
    ];

    private static readonly string[] POSITION_DESCRIPTIONS =
    [
        "Development and maintenance of software solutions", "Testing and quality assurance of products",
        "Requirements analysis and solution design", "Project and team management", "Technical customer support",
        "Business process optimization", "Information security assurance", "System and service administration",
        "Solution architecture development", "Technical consulting and advisory", "Data analysis and reporting",
        "Infrastructure monitoring and maintenance", "API design and implementation",
        "Database optimization and management", "Cloud platform administration", "DevOps pipeline automation",
        "User experience research and design", "Performance tuning and optimization",
        "Security audit and compliance", "Strategic planning and execution"
    ];

    private static readonly string[] LOCATION_TYPES =
        ["Office", "Hub", "Center", "Campus", "Facility", "Branch", "Headquarters"];

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
        var usedNames = new HashSet<string>();

        for (int i = 0; i < LOCATIONS_COUNT; i++)
        {
            string city, street, house, postalCode, addressKey, locationName;

            do
            {
                city = CITIES[_random.Next(CITIES.Length)];
                street = STREETS[_random.Next(STREETS.Length)];
                house = _random.Next(1, 500).ToString();

                // Более разнообразные почтовые индексы
                postalCode = _random.Next(10000, 99999).ToString();

                addressKey = $"{city}|{street}|{house}|{postalCode}";

                // Более разнообразные имена локаций
                string locationType = LOCATION_TYPES[_random.Next(LOCATION_TYPES.Length)];
                int suffix = _random.Next(1, 1000);
                locationName = $"{locationType} {city} {suffix}";
            } while (usedAddresses.Contains(addressKey) || usedNames.Contains(locationName));

            usedAddresses.Add(addressKey);
            usedNames.Add(locationName);

            var location = Location.Create(
                LocationId.NewId(),
                LocationName.Create(locationName).Value,
                Address.Create(city, street, postalCode, house).Value,
                LocationTimeZone.Create(TIMEZONES[_random.Next(TIMEZONES.Length)]).Value);

            // Устанавливаем случайную дату создания
            SetRandomDates(location);

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

        // Варьируем количество детей
        int childCount = _random.Next(2, CHILD_DEPARTMENTS_PER_PARENT + 1);

        for (int i = 0; i < childCount; i++)
        {
            var child = CreateDepartment(
                parent,
                locations,
                usedIdentifiers,
                currentDepth);

            allDepartments.Add(child);

            // Уменьшаем вероятность создания детей с увеличением глубины
            double probability = 0.7 - (currentDepth * 0.15);
            if (_random.NextDouble() < probability)
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
            string prefix = DEPARTMENT_PREFIXES[_random.Next(DEPARTMENT_PREFIXES.Length)];
            string baseName = DEPARTMENT_NAMES[_random.Next(DEPARTMENT_NAMES.Length)];
            string suffix = GenerateRandomLetters(_random.Next(3, 6));

            identifier = $"{prefix}-{baseName}-{suffix}"
                .ToLower()
                .Replace(" ", "-");

            name = $"{prefix} {baseName}";

            if (parent != null)
            {
                // Более разнообразные имена для дочерних департаментов
                string childSuffix = GenerateRandomLetters(2).ToUpper();
                int number = _random.Next(1, 100);
                name += $" {childSuffix}{number}";
            }
        } while (usedIdentifiers.Contains(identifier) || identifier.Length > 150 || identifier.Length < 3);

        usedIdentifiers.Add(identifier);

        // Более разнообразное количество локаций
        int departmentLocationCount = _random.Next(
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

        // Устанавливаем случайную дату создания
        SetRandomDates(department);

        return department;
    }

    private string GenerateRandomLetters(int length)
    {
        const string letters = "abcdefghijklmnopqrstuvwxyz";
        char[] result = new char[length];

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
                string baseName = POSITION_NAMES[_random.Next(POSITION_NAMES.Length)];
                string level = POSITION_LEVELS[_random.Next(POSITION_LEVELS.Length)];

                // Добавляем номер для уникальности
                int uniqueNumber = _random.Next(1, 1000);
                name = $"{level} {baseName} {uniqueNumber}";
            } while (usedNames.Contains(name));

            usedNames.Add(name);

            // Более разнообразное количество департаментов
            int departmentCount = _random.Next(
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

            // 70% шанс иметь описание
            string? description = _random.NextDouble() > 0.3
                ? POSITION_DESCRIPTIONS[_random.Next(POSITION_DESCRIPTIONS.Length)]
                : null;

            var position = Position.Create(
                positionId,
                PositionName.Create(name).Value,
                description,
                departmentPositions).Value;

            // Устанавливаем случайную дату создания
            SetRandomDates(position);

            positions.Add(position);
        }

        await _dbContext.Positions.AddRangeAsync(positions);
        await _dbContext.SaveChangesAsync();

        return positions;
    }

    private void SetRandomDates(object entity)
    {
        var createdAt = GenerateRandomDate(START_DATE, END_DATE);
        var updatedAt = GenerateRandomDate(createdAt, END_DATE);

        var entityType = entity.GetType();

        // Ищем backing fields (приватные поля с автосвойствами)
        var createdAtField =
            entityType.GetField("<CreatedAt>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        var updatedAtField =
            entityType.GetField("<UpdatedAt>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);

        // Если backing fields не найдены, пробуем найти обычные приватные поля
        if (createdAtField == null)
        {
            createdAtField = entityType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(f => f.Name.Contains("createdAt", StringComparison.OrdinalIgnoreCase));
        }

        if (updatedAtField == null)
        {
            updatedAtField = entityType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(f => f.Name.Contains("updatedAt", StringComparison.OrdinalIgnoreCase));
        }

        createdAtField?.SetValue(entity, createdAt);
        updatedAtField?.SetValue(entity, updatedAt);
    }

    private DateTime GenerateRandomDate(DateTime from, DateTime to)
    {
        var range = to - from;
        var randomTimeSpan = new TimeSpan((long)(_random.NextDouble() * range.Ticks));
        return from + randomTimeSpan;
    }
}