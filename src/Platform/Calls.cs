using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Acme.Workforce.Platform;

/// <summary>A retry policy with Polly's asynchronous callback shape and a bounded exponential backoff.</summary>
public sealed class RetryPolicy(ILogger<RetryPolicy> logger)
{
    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken token)
    {
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                return await action(token).ConfigureAwait(false);
            }
            catch (RpcException error) when (error.StatusCode == StatusCode.Unavailable && attempt < 2)
            {
                logger.LogWarning("Retrying unavailable dependency, attempt {Attempt}", attempt + 1);
                await Task.Delay(TimeSpan.FromMilliseconds(25 * (1 << attempt)), token).ConfigureAwait(false);
            }
        }
    }
}

/// <summary>Application middleware expressed as asynchronous functions, including method-group callbacks.</summary>
public static class Workflow
{
    public static async Task<TReply> SendAsync<TRequest, TReply>(TRequest request, CancellationToken token,
        Func<TRequest, CallOptions, AsyncUnaryCall<TReply>> send) where TRequest : class where TReply : class
    {
        using var call = send(request, new CallOptions(cancellationToken: token));
        return await call.ResponseAsync.ConfigureAwait(false);
    }

    public static async Task<T> RunAsync<T>(Func<CancellationToken, Task<T>> stage, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        return await TraceAsync(stage, token).ConfigureAwait(false);
    }

    private static async Task<T> TraceAsync<T>(Func<CancellationToken, Task<T>> stage, CancellationToken token) =>
        await ExecuteAsync(stage, token).ConfigureAwait(false);

    private static async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> stage, CancellationToken token) =>
        await stage(token).ConfigureAwait(false);
}

/// <summary>Marshalling for gateways that construct unary method descriptors from service and method strings.</summary>
public static class Wire
{
    public static Marshaller<T> Marshaller<T>() where T : class, IMessage<T>, new() =>
        Marshallers.Create<T>(message => message.ToByteArray(), bytes => new MessageParser<T>(() => new T()).ParseFrom(bytes));
}
