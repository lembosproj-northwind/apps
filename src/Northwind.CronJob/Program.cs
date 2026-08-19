using Northwind.Runtime;

// Stands in for any scheduled job — a migration, a nightly report. It runs once and exits, and its
// exit code is the whole contract: the CronJob's backoffLimit retries a non-zero, so a job that
// swallowed its failure and returned 0 would be a schedule that silently stops doing its work.

var identity = WorkloadIdentity.FromEnvironment();
var bindings = ResourceBindings.FromEnvironment();

Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] starting {identity}");

var unresolved = bindings.Where(binding => !binding.IsResolved).Select(binding => binding.Handle).ToArray();

if (unresolved.Length > 0)
{
    Console.Error.WriteLine(
        $"[{DateTimeOffset.UtcNow:O}] unresolved binding(s): {string.Join(", ", unresolved)}");

    // Non-zero, so the retry policy the blueprint attached actually engages.
    return 1;
}

foreach (var binding in bindings)
    Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] binding {binding.Handle} resolved");

await Task.Delay(TimeSpan.FromSeconds(2));

Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] completed {identity}");
return 0;
