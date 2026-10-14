using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiDocuments = Acme.Documents.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Ops.WorkplaceReconciler;

/// <summary>Where CarryDocumentSyncStateNumber hands each record.</summary>
internal interface ICarryDocumentSyncStateNumberDestination
{
    Task ApplyAsync(CarryDocumentSyncStateNumberRecord record, CancellationToken token);
}

/// <summary>What CarryDocumentSyncStateNumber keeps of each GetDocumentSyncState response.</summary>
internal sealed class CarryDocumentSyncStateNumberRecord
{
    public string Number { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string DocumentId { get; init; } = "";
}

/// <summary>Writes records to acme.documents.v1.DocumentFolderAdminService.</summary>
internal sealed class CarryDocumentSyncStateNumberDocumentFolderAdminTarget(ApiDocuments.DocumentFolderAdminService.DocumentFolderAdminServiceClient client) : ICarryDocumentSyncStateNumberDestination
{
    public async Task ApplyAsync(CarryDocumentSyncStateNumberRecord record, CancellationToken token)
    {
        await client.ArchiveDocumentFolderAsync(new ApiDocuments.ArchiveDocumentFolderRequest { DocumentFolderId = record.DocumentId, Details = new ApiDocuments.ArchiveDocumentFolderRequest.Types.Details { ReasonCode = record.Number }, Reason = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.scheduling.v1.ShiftApprovalService.</summary>
internal sealed class CarryDocumentSyncStateNumberShiftApprovalTarget : ICarryDocumentSyncStateNumberDestination
{
    private readonly ApiScheduling.ShiftApprovalService.ShiftApprovalServiceClient _client;

    public CarryDocumentSyncStateNumberShiftApprovalTarget(ApiScheduling.ShiftApprovalService.ShiftApprovalServiceClient client) => _client = client;

    public async Task ApplyAsync(CarryDocumentSyncStateNumberRecord record, CancellationToken token)
    {
        await _client.RecallShiftAsync(new ApiScheduling.RecallShiftRequest { ShiftId = record.Number }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.documents.v1.DocumentNotificationService.</summary>
internal sealed class CarryDocumentSyncStateNumberDocumentNotificationRelay(ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient client) : ICarryDocumentSyncStateNumberDestination
{
    public async Task ApplyAsync(CarryDocumentSyncStateNumberRecord record, CancellationToken token)
    {
        await client.SendDocumentTestNoticeAsync(new ApiDocuments.SendDocumentTestNoticeRequest { Details = new ApiDocuments.SendDocumentTestNoticeRequest.Types.Details { TemplateId = record.Number } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.documents.v1.DocumentSyncService/GetDocumentSyncState and hands what it keeps to the sink of its kind.</summary>
internal static class CarryDocumentSyncStateNumber
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDocuments.DocumentSyncService.DocumentSyncServiceClient(channel);
        var response = await source.GetDocumentSyncStateAsync(new ApiDocuments.GetDocumentSyncStateRequest() { DocumentId = "main-carry-document-sync-state-number" }, cancellationToken: token);
        var record = new CarryDocumentSyncStateNumberRecord
        {
            Number = response.Document.Number,
            CorrelationId = response.Result.CorrelationId,
            DocumentId = Text.Normalize(response.Document.DocumentId),
        };
        await ActOnCarryDocumentSyncStateNumberAsync("document-notification", record, channel, token);
    }

    private static async Task ActOnCarryDocumentSyncStateNumberAsync(string kind, CarryDocumentSyncStateNumberRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["document-folder-admin"] = () => new CarryDocumentSyncStateNumberDocumentFolderAdminTarget(new ApiDocuments.DocumentFolderAdminService.DocumentFolderAdminServiceClient(channel)).ApplyAsync(record, token),
            ["shift-approval"] = () => new CarryDocumentSyncStateNumberShiftApprovalTarget(new ApiScheduling.ShiftApprovalService.ShiftApprovalServiceClient(channel)).ApplyAsync(record, token),
            ["document-notification"] = () => new CarryDocumentSyncStateNumberDocumentNotificationRelay(new ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient(channel)).ApplyAsync(record, token),
        };
        await actions[kind]();
    }
}
