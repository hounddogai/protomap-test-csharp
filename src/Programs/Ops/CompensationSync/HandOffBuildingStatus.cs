using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiDocuments = Acme.Documents.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiPerformers = Acme.Performers.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.CompensationSync;

/// <summary>Where HandOffBuildingStatus hands each record.</summary>
internal interface IHandOffBuildingStatusWriter
{
    Task SendAsync(HandOffBuildingStatusRecord record, CancellationToken token);
}

/// <summary>What HandOffBuildingStatus keeps of each ExpireBuilding response.</summary>
internal sealed class HandOffBuildingStatusRecord
{
    public string Status { get; init; } = "";
    public string ResultStatus { get; init; } = "";
    public string Name { get; init; } = "";
}

/// <summary>Writes records to acme.itsm.v1.ChangeRequestValidationService.</summary>
internal sealed class HandOffBuildingStatusChangeRequestValidationWriter : IHandOffBuildingStatusWriter
{
    private readonly ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient _client;

    public HandOffBuildingStatusChangeRequestValidationWriter(ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient client) => _client = client;

    public async Task SendAsync(HandOffBuildingStatusRecord record, CancellationToken token)
    {
        await _client.ScoreChangeRequestAsync(new ApiItsm.ScoreChangeRequestRequest { Details = new ApiItsm.ScoreChangeRequestRequest.Types.Details { CurrencyCode = record.Name } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.documents.v1.DocumentFolderHistoryService.</summary>
internal sealed class HandOffBuildingStatusDocumentFolderHistoryTarget : IHandOffBuildingStatusWriter
{
    private readonly ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient _client;

    public HandOffBuildingStatusDocumentFolderHistoryTarget(ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient client) => _client = client;

    public async Task SendAsync(HandOffBuildingStatusRecord record, CancellationToken token)
    {
        await _client.RevertDocumentFolderAsync(new ApiDocuments.RevertDocumentFolderRequest { DocumentFolder = new ApiDocuments.DocumentFolder { DocumentFolderId = record.Status, Name = record.Name } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.performers.v1.PerformerService.</summary>
internal sealed class HandOffBuildingStatusPerformerTarget : IHandOffBuildingStatusWriter
{
    private readonly ApiPerformers.PerformerService.PerformerServiceClient _client;

    public HandOffBuildingStatusPerformerTarget(ApiPerformers.PerformerService.PerformerServiceClient client) => _client = client;

    public async Task SendAsync(HandOffBuildingStatusRecord record, CancellationToken token)
    {
        await _client.ClonePerformerAsync(new ApiPerformers.ClonePerformerRequest { Performer = new ApiPerformers.Performer { PerformerId = record.ResultStatus } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.recruiting.v1.JobOpeningCalculationService.</summary>
internal sealed class HandOffBuildingStatusJobOpeningCalculationRelay : IHandOffBuildingStatusWriter
{
    private readonly ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient _client;

    public HandOffBuildingStatusJobOpeningCalculationRelay(ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient client) => _client = client;

    public async Task SendAsync(HandOffBuildingStatusRecord record, CancellationToken token)
    {
        await _client.QuoteJobOpeningAsync(new ApiRecruiting.QuoteJobOpeningRequest { JobOpening = new ApiRecruiting.JobOpening { Notes = record.Status }, Reason = record.Name }, cancellationToken: token);
    }
}

/// <summary>Reads acme.workplaces.v1.BuildingLifecycleService/ExpireBuilding and hands what it keeps to the sink of its kind.</summary>
internal static class HandOffBuildingStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiWorkplaces.BuildingLifecycleService.BuildingLifecycleServiceClient(channel);
        var reply = await source.ExpireBuildingAsync(new ApiWorkplaces.ExpireBuildingRequest() { BuildingId = "primary-hand-off-building-status" }, cancellationToken: token);
        var record = new HandOffBuildingStatusRecord
        {
            Status = reply.Outcome.Status,
            ResultStatus = reply.Result.Status,
            Name = Text.Normalize(reply.Building.Name),
        };
        var sinks = new Dictionary<string, IHandOffBuildingStatusWriter>
        {
            ["change-request-validation"] = new HandOffBuildingStatusChangeRequestValidationWriter(new ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient(channel)),
            ["document-folder-history"] = new HandOffBuildingStatusDocumentFolderHistoryTarget(new ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient(channel)),
            ["performer"] = new HandOffBuildingStatusPerformerTarget(new ApiPerformers.PerformerService.PerformerServiceClient(channel)),
            ["job-opening-calculation"] = new HandOffBuildingStatusJobOpeningCalculationRelay(new ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient(channel)),
        };
        await RouteHandOffBuildingStatusAsync("performer", record, sinks, token);
    }

    private static async Task RouteHandOffBuildingStatusAsync(string kind, HandOffBuildingStatusRecord record, IReadOnlyDictionary<string, IHandOffBuildingStatusWriter> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.SendAsync(record, token);
    }
}
