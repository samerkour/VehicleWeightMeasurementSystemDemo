using System.Net.Sockets;
using Microsoft.Extensions.Options;
using RmtoSync.Configuration;

namespace RmtoSync.Services;

/// <summary>Probes internet connectivity (optional) and Rahdari service reachability.</summary>
public sealed class RahdariHealthCheck
{
    private readonly RahdariOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;

    public RahdariHealthCheck(
        IOptions<RahdariOptions> options,
        IHttpClientFactory httpClientFactory)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<RahdariHealthResult> CheckAsync(CancellationToken ct)
    {
        var timeout = TimeSpan.FromMilliseconds(Math.Max(_options.HealthCheckTimeoutMs, 500));

        var internet = await CheckInternetAsync(timeout, ct);
        var service = await CheckServiceAsync(timeout, ct);

        var isHealthy = (!internet.Configured || internet.Ok) && service.Ok;

        return new RahdariHealthResult(
            IsHealthy: isHealthy,
            InternetCheckConfigured: internet.Configured,
            InternetOk: internet.Ok,
            InternetMessage: internet.Message,
            ServiceOk: service.Ok,
            ServiceMessage: service.Message);
    }

    private async Task<(bool Configured, bool Ok, string Message)> CheckInternetAsync(TimeSpan timeout, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.InternetCheckUrl))
            return (false, true, "not configured");

        try
        {
            var client = _httpClientFactory.CreateClient("HealthCheck");
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeout);

            using var response = await client.GetAsync(_options.InternetCheckUrl, cts.Token);
            var ok = response.IsSuccessStatusCode;
            return (true, ok, ok ? $"HTTP {(int)response.StatusCode} OK" : $"HTTP {(int)response.StatusCode}");
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return (true, false, "timeout");
        }
        catch (Exception ex)
        {
            return (true, false, ex.Message);
        }
    }

    private async Task<(bool Ok, string Message)> CheckServiceAsync(TimeSpan timeout, CancellationToken ct)
    {
        try
        {
            var uri = new Uri(_options.ServiceUrl);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeout);

            using var tcp = new TcpClient();
            await tcp.ConnectAsync(uri.Host, uri.Port).WaitAsync(cts.Token);

            return tcp.Connected
                ? (true, $"{uri.Host}:{uri.Port} OK")
                : (false, $"connect failed {uri.Host}:{uri.Port}");
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return (false, "timeout");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}

public sealed record RahdariHealthResult(
    bool IsHealthy,
    bool InternetCheckConfigured,
    bool InternetOk,
    string? InternetMessage,
    bool ServiceOk,
    string? ServiceMessage);