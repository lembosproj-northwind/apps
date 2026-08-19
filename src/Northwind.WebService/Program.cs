using Northwind.Runtime;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var identity = WorkloadIdentity.FromEnvironment();
var bindings = ResourceBindings.FromEnvironment();

app.Logger.LogInformation("Starting {Identity} with {Count} resource binding(s).",
    identity, bindings.Count);

// Liveness is about the process, readiness is about its dependencies, and conflating them is how a
// workload with an unreachable database gets restarted forever instead of reported unhealthy.
app.MapGet("/healthz", () => Results.Ok(new { status = "alive" }));

app.MapGet("/readyz", () =>
{
    var unresolved = bindings.Where(binding => !binding.IsResolved).Select(binding => binding.Handle).ToArray();

    return unresolved.Length == 0
        ? Results.Ok(new { status = "ready" })
        : Results.Json(new { status = "unready", unresolved }, statusCode: 503);
});

// What the platform reads to learn observed state. Reporting the spec version is the whole point:
// a pod that exists proves something is running, not that the right thing is.
app.MapGet("/identity", () => Results.Ok(new
{
    identity.Component,
    identity.Environment,
    identity.Stamp,
    identity.SpecVersion,
    Bindings = bindings.Select(ResourceBindings.Describe)
}));

app.MapGet("/", () => Results.Ok(new { service = identity.Component, version = identity.SpecVersion }));

app.Run();
