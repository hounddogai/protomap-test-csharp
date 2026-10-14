using Grpc.Net.Client;
using ApiAudiences = Acme.Audiences.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiItsm = Acme.Itsm.V1;

namespace Acme.Workforce.Ops.AssetsReplicator;

/// <summary>Where MirrorAudiencesId hands each record.</summary>
internal interface IMirrorAudiencesIdDestination
{
    Task DeliverAsync(MirrorAudiencesIdRecord record, CancellationToken token);
}

/// <summary>What MirrorAudiencesId keeps of each ListAudiences response.</summary>
internal sealed class MirrorAudiencesIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Hands records to acme.itsm.v1.ChangeRequestLifecycleService.</summary>
internal sealed class MirrorAudiencesIdChangeRequestLifecycleRelay(ApiItsm.ChangeRequestLifecycleService.ChangeRequestLifecycleServiceClient client) : IMirrorAudiencesIdDestination
{
    public async Task DeliverAsync(MirrorAudiencesIdRecord record, CancellationToken token)
    {
        await client.SuspendChangeRequestAsync(new ApiItsm.SuspendChangeRequestRequest { Details = new ApiItsm.SuspendChangeRequestRequest.Types.Details { ReasonCode = record.Status }, Reason = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.documents.v1.DocumentTemplateCalculationService.</summary>
internal sealed class MirrorAudiencesIdDocumentTemplateCalculationTarget : IMirrorAudiencesIdDestination
{
    private readonly ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient _client;

    public MirrorAudiencesIdDocumentTemplateCalculationTarget(ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient client) => _client = client;

    public async Task DeliverAsync(MirrorAudiencesIdRecord record, CancellationToken token)
    {
        await _client.QuoteDocumentTemplateAsync(new ApiDocuments.QuoteDocumentTemplateRequest { DocumentTemplate = new ApiDocuments.DocumentTemplate { DocumentTemplateId = record.Status }, DocumentTemplateId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.documents.v1.DocumentFolderExportService.</summary>
internal sealed class MirrorAudiencesIdDocumentFolderExportTarget : IMirrorAudiencesIdDestination
{
    private readonly ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient _client;

    public MirrorAudiencesIdDocumentFolderExportTarget(ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient client) => _client = client;

    public async Task DeliverAsync(MirrorAudiencesIdRecord record, CancellationToken token)
    {
        await _client.ScheduleDocumentFolderExportAsync(new ApiDocuments.ScheduleDocumentFolderExportRequest { Details = new ApiDocuments.ScheduleDocumentFolderExportRequest.Types.Details { Cron = record.Status }, Reason = record.CorrelationId, DocumentFolderId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class MirrorAudiencesIdLogOnly : IMirrorAudiencesIdDestination
{
    public Task DeliverAsync(MirrorAudiencesIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.CorrelationId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.audiences.v1.AudienceService/ListAudiences and hands what it keeps to the sink of its kind.</summary>
internal static class MirrorAudiencesId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiAudiences.AudienceService.AudienceServiceClient(channel);
        var found = await source.ListAudiencesAsync(new ApiAudiences.ListAudiencesRequest() { OrderBy = "current-mirror-audiences-id" }, cancellationToken: token);
        var record = new MirrorAudiencesIdRecord
        {
            CorrelationId = found.Result.CorrelationId,
            Message = found.Result.Message,
            Status = found.Result.Status,
        };
        await RouteMirrorAudiencesIdAsync("change-request-lifecycle", record, channel, token);
    }

    private static async Task RouteMirrorAudiencesIdAsync(string kind, MirrorAudiencesIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        IMirrorAudiencesIdDestination sink = new MirrorAudiencesIdLogOnly();
        switch (kind)
        {
            case "change-request-lifecycle":
                sink = new MirrorAudiencesIdChangeRequestLifecycleRelay(new ApiItsm.ChangeRequestLifecycleService.ChangeRequestLifecycleServiceClient(channel));
                break;
            case "document-template-calculation":
                sink = new MirrorAudiencesIdDocumentTemplateCalculationTarget(new ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient(channel));
                break;
            case "document-folder-export":
                sink = new MirrorAudiencesIdDocumentFolderExportTarget(new ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient(channel));
                break;
        }
        await sink.DeliverAsync(record, token);
    }
}
