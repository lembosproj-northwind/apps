using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Northwind.Runtime;

// The serverless archetype. A custom runtime rather than a managed one, so the assembly name is
// `bootstrap` and the blueprint's default handler matches without the module knowing anything about
// .NET — which is what keeps `serverless-function` a platform blueprint rather than a .NET one.

var identity = WorkloadIdentity.FromEnvironment();
var bindings = ResourceBindings.FromEnvironment();

// The type arguments are spelled out rather than inferred: Create has thirty overloads, and a method
// group returning object matches several of them ambiguously.
await LambdaBootstrapBuilder
    .Create<InvocationRequest, InvocationResponse>(Handle, new DefaultLambdaJsonSerializer())
    .Build()
    .RunAsync();

return;

InvocationResponse Handle(InvocationRequest request, ILambdaContext context)
{
    var unresolved = bindings
        .Where(binding => !binding.IsResolved)
        .Select(binding => binding.Handle)
        .ToArray();

    context.Logger.LogInformation($"{identity} handled {request.Kind ?? "invoke"}");

    return new InvocationResponse(
        identity.Component,
        identity.Environment,
        identity.Stamp,
        identity.SpecVersion,
        unresolved);
}

/// <summary>What the function accepts. Deliberately loose — the archetype is not the payload.</summary>
public sealed record InvocationRequest(string? Kind);

/// <summary>
/// What it answers with: the same identity the web service serves on <c>/identity</c>.
/// </summary>
/// <remarks>
/// A Lambda has no port to probe, so the response is the only place observed state can come from —
/// which is why the spec version is in it rather than only in the logs.
/// </remarks>
public sealed record InvocationResponse(
    string Component,
    string Environment,
    string Stamp,
    string SpecVersion,
    IReadOnlyList<string> UnresolvedBindings);
