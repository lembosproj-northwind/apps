using Northwind.Runtime;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<ConsumerService>();
builder.Build().Run();

/// <summary>
/// Stands in for any queue or stream consumer. It consumes nothing; what it demonstrates is the
/// shape — identity reported on start, work logged on a loop, and a shutdown that finishes the
/// message in flight rather than dropping it.
/// </summary>
internal sealed class ConsumerService(ILogger<ConsumerService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var identity = WorkloadIdentity.FromEnvironment();
        var bindings = ResourceBindings.FromEnvironment();

        logger.LogInformation("Consumer {Identity} started with {Count} binding(s).", identity, bindings.Count);

        foreach (var binding in bindings.Where(b => !b.IsResolved))
            logger.LogWarning("Binding {Handle} did not resolve; work needing it will fail.", binding.Handle);

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Polled. component={Component} spec={SpecVersion} stamp={Stamp}",
                identity.Component, identity.SpecVersion, identity.Stamp);

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Shutdown, not a fault. Falling through lets the loop condition end it cleanly.
            }
        }

        logger.LogInformation("Consumer {Identity} stopped.", identity);
    }
}
