using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiCommon = Acme.Common.V1;
using ApiIdentity = Acme.Identity.V1;

namespace Acme.Workforce.Platform;

/// <summary>
/// Authenticates unary and streaming requests before any workforce endpoint runs.
/// </summary>
public sealed class IdentityInterceptor(ApiIdentity.UserQueryService.UserQueryServiceClient identity, ILogger<IdentityInterceptor> logger) : Interceptor
{
    private async Task<string> AuthenticateAsync(ServerCallContext context)
    {
        var authorization = context.RequestHeaders.GetValue("authorization") ?? "";
        var tenant = context.RequestHeaders.GetValue("x-tenant-id") ?? "";
        if (!authorization.StartsWith("Bearer ", StringComparison.Ordinal) || authorization.Length == 7 || tenant.Length == 0)
            throw new RpcException(new Status(StatusCode.Unauthenticated, "A bearer token and a tenant are required"));
        var token = authorization[7..];
        var request = new ApiIdentity.LookupUserRequest
        {
            Context = new ApiCommon.RequestContext
            {
                Actor = new ApiCommon.Actor
                {
                    ServiceAccount = true,
                    UserId = "workforce",
                },
                TenantId = tenant,
            },
            UserId = token,
        };
        ApiIdentity.LookupUserResponse reply;
        try
        {
            // The identity client signs the lookup, the server's own call, with its workload credential.
            using var call = identity.LookupUserAsync(request, cancellationToken: context.CancellationToken);
            reply = await call.ResponseAsync;
        }
        catch (RpcException error)
        {
            logger.LogWarning("Identity rejected call {Method}: {Status}", context.Method, error.StatusCode);
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Identity could not authenticate the caller"));
        }
        if (reply.Result?.Status != "ok" || reply.User?.UserId != token)
            throw new RpcException(new Status(StatusCode.PermissionDenied, "Identity rejected the caller"));
        return tenant;
    }
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        CallerTenant.Current = await AuthenticateAsync(context);
        return await continuation(request, context);
    }
    public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
        TRequest request,
        IServerStreamWriter<TResponse> response,
        ServerCallContext context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        CallerTenant.Current = await AuthenticateAsync(context);
        await continuation(request, response, context);
    }
    public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> request,
        ServerCallContext context,
        ClientStreamingServerMethod<TRequest, TResponse> continuation)
    {
        CallerTenant.Current = await AuthenticateAsync(context);
        return await continuation(request, context);
    }
    public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> request,
        IServerStreamWriter<TResponse> response,
        ServerCallContext context,
        DuplexStreamingServerMethod<TRequest, TResponse> continuation)
    {
        CallerTenant.Current = await AuthenticateAsync(context);
        await continuation(request, response, context);
    }
}
