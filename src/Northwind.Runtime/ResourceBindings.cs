namespace Northwind.Runtime;

/// <summary>
/// The resources this workload was bound to, by the handle its ComponentSpec declared.
/// </summary>
/// <param name="Handle">The spec's local name for the need — <c>ordersDb</c>.</param>
/// <param name="Url">The connection string, when the platform mounted one.</param>
/// <param name="SecretRef">The path to resolve at start-up, when it passed a reference instead.</param>
public sealed record ResourceBinding(string Handle, string? Url, string? SecretRef)
{
    /// <summary>
    /// Whether this binding can actually be used. A reference with nothing behind it is not resolved.
    /// </summary>
    public bool IsResolved => !string.IsNullOrWhiteSpace(Url);
}

public static class ResourceBindings
{
    /// <summary>
    /// Every binding the environment carries, in either spelling.
    /// </summary>
    /// <remarks>
    /// Two spellings because two provisioners: Helm mounts the credential as <c>&lt;HANDLE&gt;_URL</c>
    /// from a secret the provisioning run placed, while Terraform passes
    /// <c>&lt;HANDLE&gt;_SECRET_REF</c> and leaves resolution to the runtime. Reading only one would
    /// make the same ComponentSpec work under one blueprint and silently lose its database under the
    /// other.
    /// </remarks>
    public static IReadOnlyList<ResourceBinding> FromEnvironment()
    {
        var urls = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var refs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (System.Collections.DictionaryEntry entry in System.Environment.GetEnvironmentVariables())
        {
            var key = entry.Key.ToString();
            var value = entry.Value?.ToString();
            if (key is null || string.IsNullOrWhiteSpace(value))
                continue;

            if (key.EndsWith("_URL", StringComparison.Ordinal))
                urls[key[..^4]] = value;
            else if (key.EndsWith("_SECRET_REF", StringComparison.Ordinal))
                refs[key[..^11]] = value;
        }

        return urls.Keys.Union(refs.Keys, StringComparer.OrdinalIgnoreCase)
            .OrderBy(handle => handle, StringComparer.OrdinalIgnoreCase)
            .Select(handle => new ResourceBinding(
                handle,
                urls.GetValueOrDefault(handle),
                refs.GetValueOrDefault(handle)))
            .ToList();
    }

    /// <summary>
    /// A redacted view. The url is never reported — only that one arrived.
    /// </summary>
    /// <remarks>
    /// The identity endpoint exists to be read by anything that can reach the pod, so it must not become
    /// the one place a connection string with a password in it is served over plain HTTP.
    /// </remarks>
    public static object Describe(ResourceBinding binding) => new
    {
        binding.Handle,
        binding.IsResolved,
        binding.SecretRef,
        Url = binding.IsResolved ? "***" : null
    };
}
