using Microsoft.Data.SqlClient;
using RmtoSync.Data;

namespace RmtoSync.Services;

public sealed class DatabaseHealthService : IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseHealthService> _logger;

    public DatabaseHealthService(IConfiguration configuration, ILogger<DatabaseHealthService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _logger.LogCritical("Connection string 'DefaultConnection' is missing");
            return;
        }

        try
        {

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            _logger.LogInformation("Ok, SQL connetion to Database={Database}", connection.Database);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Cannot connect to Database");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
