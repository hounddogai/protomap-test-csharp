using Grpc.Core;
using Acme.Common.V1;
using System.Diagnostics.CodeAnalysis;

namespace Acme.Workforce.Platform;

/// <summary>Settings supplied by the deployment environment or the operator's configuration.</summary>
public sealed class MeshOptions
{
    public string Address { get; set; } = "http://localhost:15001";
    public string ServiceToken { get; set; } = "";
    public string Region { get; set; } = "local";
}

/// <summary>Request validation shared by the domain endpoints and their background consumers.</summary>
public static class RequestGuard
{
    public static void Tenant([NotNull] RequestContext? context)
    {
        if (string.IsNullOrWhiteSpace(context?.TenantId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "context.tenant_id is required"));
        if (context.TenantId.Length > 128)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "tenant identifier is too long"));
    }

    public static void Identifier(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new RpcException(new Status(StatusCode.InvalidArgument, field + " is required"));
    }
}
