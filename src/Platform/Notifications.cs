using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Acme.Workforce.Platform;

/// <summary>Notifications are queued in memory; publishing does not await the consumer's RPC.</summary>
public interface INotification { }

public sealed record TransportNotification<T>(string Topic, T Request) : INotification;

public interface INotificationHandler
{
    string Topic { get; }
    Task HandleAsync(INotification notification, CancellationToken token);
}

/// <summary>A bounded in-process notification mediator with one queue per subscribed workflow.</summary>
public sealed class NotificationBus
{
    private readonly ConcurrentDictionary<string, Channel<INotification>> _queues = new(StringComparer.Ordinal);

    private Channel<INotification> Queue(string topic) => _queues.GetOrAdd(topic, _ =>
        Channel.CreateBounded<INotification>(new BoundedChannelOptions(128)
        {
            SingleReader = true,
            FullMode = BoundedChannelFullMode.Wait
        }));

    public ValueTask PublishAsync<T>(TransportNotification<T> notification, CancellationToken token) =>
        Queue(notification.Topic).Writer.WriteAsync(notification, token);

    public IAsyncEnumerable<INotification> ReadAllAsync(string topic, CancellationToken token) =>
        Queue(topic).Reader.ReadAllAsync(token);
}

/// <summary>Consumes notifications in the host independently of the requests that published them.</summary>
public sealed class NotificationWorker(NotificationBus bus, IEnumerable<INotificationHandler> handlers,
    ILogger<NotificationWorker> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        Task.WhenAll(handlers.Select(handler => ConsumeAsync(handler, stoppingToken)));

    private async Task ConsumeAsync(INotificationHandler handler, CancellationToken token)
    {
        await foreach (var notification in bus.ReadAllAsync(handler.Topic, token))
        {
            try
            {
                await handler.HandleAsync(notification, token);
            }
            catch (Exception error) when (error is not OperationCanceledException)
            {
                logger.LogError(error, "Notification failed for {Topic}", handler.Topic);
            }
        }
    }
}
