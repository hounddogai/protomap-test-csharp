using Acme.Common.V1;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Options;

namespace Acme.Workforce.Platform;

/// <summary>Maps deployment service names to clients created by GrpcClientFactory.</summary>
public sealed class ClientRegistry(IReadOnlyDictionary<string, object> clients)
{
    public T Get<T>(string name) where T : class => clients.TryGetValue(name, out var value) && value is T client
        ? client
        : throw new InvalidOperationException("No typed client registered for " + name);
}

/// <summary>The tenant of the call a handler serves, which the calls it makes carry when their request has none.</summary>
public static class CallerTenant
{
    private static readonly AsyncLocal<string?> Value = new();

    public static string? Current
    {
        get => Value.Value;
        set => Value.Value = value;
    }
}

/// <summary>Attaches the workload identity configured by the deployment, and the tenant every server of the mesh
/// needs, to every outgoing RPC.</summary>
public sealed class WorkloadIdentityInterceptor(IOptions<MeshOptions> options) : Interceptor
{
    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        return continuation(request, Authorize(context, request));
    }

    public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation) =>
        continuation(request, Authorize(context, request));

    public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation) => continuation(Authorize(context));

    public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation) => continuation(Authorize(context));

    private ClientInterceptorContext<TRequest, TResponse> Authorize<TRequest, TResponse>(
        ClientInterceptorContext<TRequest, TResponse> context, TRequest? request = null)
        where TRequest : class where TResponse : class
    {
        var headers = new Metadata();
        if (context.Options.Headers is { } existing)
            foreach (var entry in existing)
                headers.Add(entry);
        if (!string.IsNullOrEmpty(options.Value.ServiceToken) && headers.Get("authorization") is null)
            headers.Add("authorization", "Bearer " + options.Value.ServiceToken);
        var tenant = TenantOf(request) ?? CallerTenant.Current;
        if (!string.IsNullOrEmpty(tenant) && headers.Get("x-tenant-id") is null)
            headers.Add("x-tenant-id", tenant);
        return new(context.Method, context.Host, context.Options.WithHeaders(headers));
    }

    private static string? TenantOf(object? request) =>
        request is IMessage message
            && message.Descriptor.FindFieldByName("context")?.Accessor.GetValue(message) is RequestContext scoped
            && !string.IsNullOrEmpty(scoped.TenantId)
            ? scoped.TenantId
            : null;
}
