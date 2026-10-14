using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiAnomalies = Acme.Anomalies.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiPerformance = Acme.Performance.V1;

namespace Acme.Workforce.Ops.CompensationImporter;

/// <summary>Where BackfillOrgUnitName hands each record.</summary>
internal interface IBackfillOrgUnitNameDestination
{
    Task SendAsync(BackfillOrgUnitNameRecord record, CancellationToken token);
}

/// <summary>What BackfillOrgUnitName keeps of each SubmitOrgUnit response.</summary>
internal sealed class BackfillOrgUnitNameRecord
{
    public string Name { get; init; } = "";
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
    public string ReferenceId { get; init; } = "";
}

/// <summary>Writes records to acme.documents.v1.DocumentSyncService.</summary>
internal sealed class BackfillOrgUnitNameDocumentSyncTarget : IBackfillOrgUnitNameDestination
{
    private readonly ApiDocuments.DocumentSyncService.DocumentSyncServiceClient _client;

    public BackfillOrgUnitNameDocumentSyncTarget(ApiDocuments.DocumentSyncService.DocumentSyncServiceClient client) => _client = client;

    public async Task SendAsync(BackfillOrgUnitNameRecord record, CancellationToken token)
    {
        await _client.ReconcileDocumentsAsync(new ApiDocuments.ReconcileDocumentsRequest { Document = new ApiDocuments.Document { Number = record.Message }, Reason = record.Status }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.anomalies.v1.AnomalyService.</summary>
internal sealed class BackfillOrgUnitNameAnomalyWriter(ApiAnomalies.AnomalyService.AnomalyServiceClient client) : IBackfillOrgUnitNameDestination
{
    public async Task SendAsync(BackfillOrgUnitNameRecord record, CancellationToken token)
    {
        await client.SearchAnomaliesAsync(new ApiAnomalies.SearchAnomaliesRequest { Page = new ApiCommon.PageRequest { PageToken = record.ReferenceId }, Filter = new ApiAnomalies.SearchAnomaliesRequest.Types.Filter { OwnerId = record.Status, Query = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.performance.v1.GoalQueryService.</summary>
internal sealed class BackfillOrgUnitNameGoalQueryTarget : IBackfillOrgUnitNameDestination
{
    private readonly ApiPerformance.GoalQueryService.GoalQueryServiceClient _client;

    public BackfillOrgUnitNameGoalQueryTarget(ApiPerformance.GoalQueryService.GoalQueryServiceClient client) => _client = client;

    public async Task SendAsync(BackfillOrgUnitNameRecord record, CancellationToken token)
    {
        await _client.SummarizeGoalsAsync(new ApiPerformance.SummarizeGoalsRequest { Details = new ApiPerformance.SummarizeGoalsRequest.Types.Details { Cron = record.Status, Format = record.Name }, Reason = record.Message }, cancellationToken: token);
    }
}

/// <summary>Reads acme.employees.v1.OrgUnitApprovalService/SubmitOrgUnit and hands what it keeps to the sink of its kind.</summary>
internal static class BackfillOrgUnitName
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiEmployees.OrgUnitApprovalService.OrgUnitApprovalServiceClient(channel);
        var reply = await source.SubmitOrgUnitAsync(new ApiEmployees.SubmitOrgUnitRequest() { OrgUnitId = "main-backfill-org-unit-name" }, cancellationToken: token);
        var record = new BackfillOrgUnitNameRecord
        {
            Name = Text.Normalize(reply.OrgUnit.Name),
            Message = reply.Outcome.Message,
            Status = reply.Result.Status,
            ReferenceId = reply.Outcome.ReferenceId,
        };
        var sinks = new Dictionary<string, IBackfillOrgUnitNameDestination>
        {
            ["document-sync"] = new BackfillOrgUnitNameDocumentSyncTarget(new ApiDocuments.DocumentSyncService.DocumentSyncServiceClient(channel)),
            ["anomaly"] = new BackfillOrgUnitNameAnomalyWriter(new ApiAnomalies.AnomalyService.AnomalyServiceClient(channel)),
            ["goal-query"] = new BackfillOrgUnitNameGoalQueryTarget(new ApiPerformance.GoalQueryService.GoalQueryServiceClient(channel)),
        };
        await RouteBackfillOrgUnitNameAsync("anomaly", record, sinks, token);
    }

    private static async Task RouteBackfillOrgUnitNameAsync(string kind, BackfillOrgUnitNameRecord record, IReadOnlyDictionary<string, IBackfillOrgUnitNameDestination> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.SendAsync(record, token);
    }
}
