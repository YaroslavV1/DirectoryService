using DirectoryService.Application.Departments.DeleteInactiveDepartment;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.BackgroundServices;

public class DepartmentCleanupBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DepartmentCleanupBackgroundService> _logger;

    public DepartmentCleanupBackgroundService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<DepartmentCleanupBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int intervalHours = _configuration.GetValue<int>("DepartmentCleanup:IntervalHours");

        _logger.LogInformation(
            "Department cleanup service started with interval: {IntervalHours} hours",
            intervalHours);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _serviceProvider.CreateAsyncScope();

                var deleteDepartmentHandler =
                    scope.ServiceProvider.GetRequiredService<DeleteInactiveDepartmentHandler>();

                await deleteDepartmentHandler.Handler(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during department cleanup");
            }

            await Task.Delay(TimeSpan.FromHours(intervalHours), stoppingToken);
        }
    }
}