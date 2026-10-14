using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiEmployees = Acme.Employees.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMedia = Acme.Media.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.EngagementExporter;

/// <summary>Where SettleRosterMessage hands each record.</summary>
internal interface ISettleRosterMessageTarget
{
    Task DeliverAsync(SettleRosterMessageRecord record, CancellationToken token);
}

/// <summary>What SettleRosterMessage keeps of each QuoteRoster response.</summary>
internal sealed class SettleRosterMessageRecord
{
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Sends records to acme.workplaces.v1.VisitorPassSyncService.</summary>
internal sealed class SettleRosterMessageVisitorPassSyncWriter(ApiWorkplaces.VisitorPassSyncService.VisitorPassSyncServiceClient client) : ISettleRosterMessageTarget
{
    public async Task DeliverAsync(SettleRosterMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiWorkplaces.SyncVisitorPassesRequest { Details = new ApiWorkplaces.SyncVisitorPassesRequest.Types.Details { Checksum = record.Message }, VisitorPassId = record.CorrelationId }, token, client.SyncVisitorPassesAsync);
    }
}

/// <summary>Passes records to acme.employees.v1.OrgUnitQueryService.</summary>
internal sealed class SettleRosterMessageOrgUnitQueryRelay(ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient client) : ISettleRosterMessageTarget
{
    public async Task DeliverAsync(SettleRosterMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiEmployees.SummarizeOrgUnitsRequest { Details = new ApiEmployees.SummarizeOrgUnitsRequest.Types.Details { Format = record.CorrelationId } }, token, client.SummarizeOrgUnitsAsync);
    }
}

/// <summary>Hands records to acme.media.v1.MediaAssetService.</summary>
internal sealed class SettleRosterMessageMediaAssetTarget(ApiMedia.MediaAssetService.MediaAssetServiceClient client) : ISettleRosterMessageTarget
{
    public async Task DeliverAsync(SettleRosterMessageRecord record, CancellationToken token)
    {
        await client.ListMediaAssetsAsync(new ApiMedia.ListMediaAssetsRequest { OrderBy = record.Message, Filter = new ApiMedia.ListMediaAssetsRequest.Types.Filter { Query = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.itsm.v1.IncidentQueryService.</summary>
internal sealed class SettleRosterMessageIncidentQuerySink(ApiItsm.IncidentQueryService.IncidentQueryServiceClient client) : ISettleRosterMessageTarget
{
    public async Task DeliverAsync(SettleRosterMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiItsm.SuggestIncidentsRequest { Filter = new ApiItsm.SuggestIncidentsRequest.Types.Filter { OwnerId = record.CorrelationId } }, token, client.SuggestIncidentsAsync);
    }
}

/// <summary>Reads acme.scheduling.v1.RosterCalculationService/QuoteRoster and hands what it keeps to the sink of its kind.</summary>
internal static class SettleRosterMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiScheduling.RosterCalculationService.RosterCalculationServiceClient(channel);
        var response = await source.QuoteRosterAsync(new ApiScheduling.QuoteRosterRequest() { RosterId = "nightly-settle-roster-message" }, cancellationToken: token);
        var record = new SettleRosterMessageRecord
        {
            Message = Text.Normalize(response.Result.Message),
            CorrelationId = Text.Normalize(response.Result.CorrelationId),
        };
        var sinks = new Dictionary<string, ISettleRosterMessageTarget>
        {
            ["visitor-pass-sync"] = new SettleRosterMessageVisitorPassSyncWriter(new ApiWorkplaces.VisitorPassSyncService.VisitorPassSyncServiceClient(channel)),
            ["org-unit-query"] = new SettleRosterMessageOrgUnitQueryRelay(new ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient(channel)),
            ["media-asset"] = new SettleRosterMessageMediaAssetTarget(new ApiMedia.MediaAssetService.MediaAssetServiceClient(channel)),
            ["incident-query"] = new SettleRosterMessageIncidentQuerySink(new ApiItsm.IncidentQueryService.IncidentQueryServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "incident-query")
                continue;
            await sink.DeliverAsync(record, token);
        }
    }
}
