using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Ops.AssetsReplicator;

/// <summary>Where SyncJobOpeningWorkflowStatus hands each record.</summary>
internal interface ISyncJobOpeningWorkflowStatusSink
{
    Task DeliverAsync(SyncJobOpeningWorkflowStatusRecord record, CancellationToken token);
}

/// <summary>What SyncJobOpeningWorkflowStatus keeps of each ResumeJobOpeningWorkflow response.</summary>
internal sealed class SyncJobOpeningWorkflowStatusRecord
{
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string JobOpeningId { get; init; } = "";
    public string ReferenceId { get; init; } = "";
}

/// <summary>Writes records to acme.documents.v1.DocumentFolderNotificationService.</summary>
internal sealed class SyncJobOpeningWorkflowStatusDocumentFolderNotificationWriter : ISyncJobOpeningWorkflowStatusSink
{
    private readonly ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient _client;

    public SyncJobOpeningWorkflowStatusDocumentFolderNotificationWriter(ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient client) => _client = client;

    public async Task DeliverAsync(SyncJobOpeningWorkflowStatusRecord record, CancellationToken token)
    {
        await _client.SubscribeDocumentFolderUpdatesAsync(new ApiDocuments.SubscribeDocumentFolderUpdatesRequest { Reason = record.ReferenceId }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.devices.v1.ManagedDeviceWorkflowService.</summary>
internal sealed class SyncJobOpeningWorkflowStatusManagedDeviceWorkflowRelay(ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient client) : ISyncJobOpeningWorkflowStatusSink
{
    public async Task DeliverAsync(SyncJobOpeningWorkflowStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiDevices.InspectManagedDeviceWorkflowRequest { ManagedDeviceId = record.JobOpeningId }, token, client.InspectManagedDeviceWorkflowAsync);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class SyncJobOpeningWorkflowStatusLogOnly : ISyncJobOpeningWorkflowStatusSink
{
    public Task DeliverAsync(SyncJobOpeningWorkflowStatusRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Status}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.recruiting.v1.JobOpeningWorkflowService/ResumeJobOpeningWorkflow and hands what it keeps to the sink of its kind.</summary>
internal static class SyncJobOpeningWorkflowStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiRecruiting.JobOpeningWorkflowService.JobOpeningWorkflowServiceClient(channel);
        var got = await source.ResumeJobOpeningWorkflowAsync(new ApiRecruiting.ResumeJobOpeningWorkflowRequest() { Reason = "nightly-sync-job-opening-workflow-status" }, cancellationToken: token);
        var record = new SyncJobOpeningWorkflowStatusRecord
        {
            Status = Text.Normalize(got.Outcome.Status),
            CorrelationId = got.Result.CorrelationId,
            JobOpeningId = Text.Normalize(got.JobOpening.JobOpeningId),
            ReferenceId = got.Outcome.ReferenceId,
        };
        await RouteSyncJobOpeningWorkflowStatusAsync("managed-device-workflow", record, channel, token);
    }

    private static async Task RouteSyncJobOpeningWorkflowStatusAsync(string kind, SyncJobOpeningWorkflowStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        ISyncJobOpeningWorkflowStatusSink sink = kind switch
        {
            "document-folder-notification" => new SyncJobOpeningWorkflowStatusDocumentFolderNotificationWriter(new ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient(channel)),
            "managed-device-workflow" => new SyncJobOpeningWorkflowStatusManagedDeviceWorkflowRelay(new ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient(channel)),
            _ => new SyncJobOpeningWorkflowStatusLogOnly(),
        };
        await sink.DeliverAsync(record, token);
    }
}
