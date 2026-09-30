using Microsoft.Data.SqlClient;
using RmtoSync.Data;

namespace RmtoSync.Services;

public sealed class DatabaseHealthService : IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly SchemaValidator _schemaValidator;
    private readonly ILogger<DatabaseHealthService> _logger;

    public DatabaseHealthService(
        IConfiguration configuration,
        SchemaValidator schemaValidator,
        ILogger<DatabaseHealthService> logger)
    {
        _configuration = configuration;
        _schemaValidator = schemaValidator;
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
            return;
        }

        // بررسی هم‌خوانی مدل EF با اسکیمای واقعی. بدون این مرحله، ناسازگاری اسکیما فقط
        // به‌شکل «Invalid column name 'X'» در دل کوئری‌ها ظاهر می‌شود و اپلیکیشن
        // بدون هیچ راهنمایی بالا می‌آید ولی هیچ تصویری ارسال نمی‌کند.
        await ValidateSchemaAsync(cancellationToken);
    }

    private async Task ValidateSchemaAsync(CancellationToken cancellationToken)
    {
        try
        {
            var report = await _schemaValidator.ValidateAsync(cancellationToken);

            if (report.IsCompatible)
            {
                _logger.LogInformation(
                    "Ok, schema matches the EF model ({Objects})",
                    string.Join(", ", report.Objects.Select(o => o.ObjectName)));
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // خاموشی در حال اجرا؛ چیزی گزارش نمی‌شود.
        }
        catch (Exception ex)
        {
            // اعتبارسنجی نباید مانع بالا آمدن سرویس شود؛ SchemaValidator خودش موارد
            // ناسازگار را با جزئیات لاگ کرده و کار در چرخه‌های بعدی ادامه می‌یابد.
            _logger.LogError(ex, "Schema validation could not complete; continuing without it");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
