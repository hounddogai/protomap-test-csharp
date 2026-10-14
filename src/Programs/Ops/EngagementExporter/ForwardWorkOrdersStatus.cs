using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiMaintenance = Acme.Maintenance.V1;
using ApiSecrets = Acme.Secrets.V1;

namespace Acme.Workforce.Ops.EngagementExporter;

/// <summary>Where ForwardWorkOrdersStatus hands each record.</summary>
internal interface IForwardWorkOrdersStatusSink
{
    Task PushAsync(ForwardWorkOrdersStatusRecord record, CancellationToken token);
}

/// <summary>What ForwardWorkOrdersStatus keeps of each ListWorkOrders response.</summary>
internal sealed class ForwardWorkOrdersStatusRecord
{
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Writes records to acme.documents.v1.DocumentNotificationService.</summary>
internal sealed class ForwardWorkOrdersStatusDocumentNotificationSink : IForwardWorkOrdersStatusSink
{
    private readonly ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient _client;

    public ForwardWorkOrdersStatusDocumentNotificationSink(ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient client) => _client = client;

    public async Task PushAsync(ForwardWorkOrdersStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiDocuments.SubscribeDocumentUpdatesRequest { DocumentId = record.Message }, token, _client.SubscribeDocumentUpdatesAsync);
    }
}

/// <summary>Sends records to acme.secrets.v1.SecretService.</summary>
internal sealed class ForwardWorkOrdersStatusSecretTarget(ApiSecrets.SecretService.SecretServiceClient client) : IForwardWorkOrdersStatusSink
{
    public async Task PushAsync(ForwardWorkOrdersStatusRecord record, CancellationToken token)
    {
        await client.ListSecretsAsync(new ApiSecrets.ListSecretsRequest { Page = new ApiCommon.PageRequest { PageToken = record.Status }, OrderBy = record.Message }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class ForwardWorkOrdersStatusLogOnly : IForwardWorkOrdersStatusSink
{
    public Task PushAsync(ForwardWorkOrdersStatusRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Status}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.maintenance.v1.WorkOrderService/ListWorkOrders and hands what it keeps to the sink of its kind.</summary>
internal static class ForwardWorkOrdersStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiMaintenance.WorkOrderService.WorkOrderServiceClient(channel);
        var found = await source.ListWorkOrdersAsync(new ApiMaintenance.ListWorkOrdersRequest() { OrderBy = "live-forward-work-orders-status" }, cancellationToken: token);
        var record = new ForwardWorkOrdersStatusRecord
        {
            Status = found.Result.Status,
            Message = Text.Normalize(found.Result.Message),
        };
        await DispatchForwardWorkOrdersStatusAsync("secret", record, channel, token);
    }

    private static async Task DispatchForwardWorkOrdersStatusAsync(string kind, ForwardWorkOrdersStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<ForwardWorkOrdersStatusRecord, CancellationToken, Task> write = new ForwardWorkOrdersStatusLogOnly().PushAsync;
        switch (kind)
        {
            case "document-notification":
                write = new ForwardWorkOrdersStatusDocumentNotificationSink(new ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient(channel)).PushAsync;
                break;
            case "secret":
                write = new ForwardWorkOrdersStatusSecretTarget(new ApiSecrets.SecretService.SecretServiceClient(channel)).PushAsync;
                break;
        }
        await write(record, token);
    }
}
