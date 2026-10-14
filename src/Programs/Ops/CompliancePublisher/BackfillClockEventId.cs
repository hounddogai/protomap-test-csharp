using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiDatasets = Acme.Datasets.V1;
using ApiFeaturestore = Acme.Featurestore.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.CompliancePublisher;

/// <summary>Where BackfillClockEventId hands each record.</summary>
internal interface IBackfillClockEventIdSink
{
    Task DeliverAsync(BackfillClockEventIdRecord record, CancellationToken token);
}

/// <summary>What BackfillClockEventId keeps of each ScoreClockEvent response.</summary>
internal sealed class BackfillClockEventIdRecord
{
    public string ClockEventId { get; init; } = "";
    public string Kind { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Sends records to acme.itsm.v1.IncidentWorkflowService.</summary>
internal sealed class BackfillClockEventIdIncidentWorkflowSink : IBackfillClockEventIdSink
{
    private readonly ApiItsm.IncidentWorkflowService.IncidentWorkflowServiceClient _client;

    public BackfillClockEventIdIncidentWorkflowSink(ApiItsm.IncidentWorkflowService.IncidentWorkflowServiceClient client) => _client = client;

    public async Task DeliverAsync(BackfillClockEventIdRecord record, CancellationToken token)
    {
        await _client.ListIncidentWorkflowsAsync(new ApiItsm.ListIncidentWorkflowsRequest { OrderBy = record.Kind, Filter = new ApiItsm.ListIncidentWorkflowsRequest.Types.Filter { OwnerId = record.CorrelationId, Query = record.ClockEventId } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.workplaces.v1.VisitorPassApprovalService.</summary>
internal sealed class BackfillClockEventIdVisitorPassApprovalWriter : IBackfillClockEventIdSink
{
    private readonly ApiWorkplaces.VisitorPassApprovalService.VisitorPassApprovalServiceClient _client;

    public BackfillClockEventIdVisitorPassApprovalWriter(ApiWorkplaces.VisitorPassApprovalService.VisitorPassApprovalServiceClient client) => _client = client;

    public async Task DeliverAsync(BackfillClockEventIdRecord record, CancellationToken token)
    {
        await _client.GetVisitorPassApprovalStatusAsync(new ApiWorkplaces.GetVisitorPassApprovalStatusRequest { VisitorPassId = record.Kind }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.datasets.v1.DatasetQueryService.</summary>
internal sealed class BackfillClockEventIdDatasetQuerySink : IBackfillClockEventIdSink
{
    private readonly ApiDatasets.DatasetQueryService.DatasetQueryServiceClient _client;

    public BackfillClockEventIdDatasetQuerySink(ApiDatasets.DatasetQueryService.DatasetQueryServiceClient client) => _client = client;

    public async Task DeliverAsync(BackfillClockEventIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiDatasets.SuggestDatasetsRequest { Filter = new ApiDatasets.SuggestDatasetsRequest.Types.Filter { Query = record.CorrelationId } }, token, _client.SuggestDatasetsAsync);
    }
}

/// <summary>Sends records to acme.featurestore.v1.FeatureViewCalculationService.</summary>
internal sealed class BackfillClockEventIdFeatureViewCalculationSink(ApiFeaturestore.FeatureViewCalculationService.FeatureViewCalculationServiceClient client) : IBackfillClockEventIdSink
{
    public async Task DeliverAsync(BackfillClockEventIdRecord record, CancellationToken token)
    {
        await client.GetFeatureViewRatesAsync(new ApiFeaturestore.GetFeatureViewRatesRequest { FeatureViewId = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class BackfillClockEventIdLogOnly : IBackfillClockEventIdSink
{
    public Task DeliverAsync(BackfillClockEventIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.ClockEventId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.timekeeping.v1.ClockEventValidationService/ScoreClockEvent and hands what it keeps to the sink of its kind.</summary>
internal static class BackfillClockEventId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient(channel);
        var got = await source.ScoreClockEventAsync(new ApiTimekeeping.ScoreClockEventRequest() { ClockEventId = "nightly-backfill-clock-event-id" }, cancellationToken: token);
        var record = new BackfillClockEventIdRecord
        {
            ClockEventId = Text.Normalize(got.ClockEvent.ClockEventId),
            Kind = got.ClockEvent.Kind,
            CorrelationId = Text.Normalize(got.Result.CorrelationId),
        };
        await DispatchBackfillClockEventIdAsync("feature-view-calculation", record, channel, token);
    }

    private static async Task DispatchBackfillClockEventIdAsync(string kind, BackfillClockEventIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<BackfillClockEventIdRecord, CancellationToken, Task> write = new BackfillClockEventIdLogOnly().DeliverAsync;
        switch (kind)
        {
            case "incident-workflow":
                write = new BackfillClockEventIdIncidentWorkflowSink(new ApiItsm.IncidentWorkflowService.IncidentWorkflowServiceClient(channel)).DeliverAsync;
                break;
            case "visitor-pass-approval":
                write = new BackfillClockEventIdVisitorPassApprovalWriter(new ApiWorkplaces.VisitorPassApprovalService.VisitorPassApprovalServiceClient(channel)).DeliverAsync;
                break;
            case "dataset-query":
                write = new BackfillClockEventIdDatasetQuerySink(new ApiDatasets.DatasetQueryService.DatasetQueryServiceClient(channel)).DeliverAsync;
                break;
            case "feature-view-calculation":
                write = new BackfillClockEventIdFeatureViewCalculationSink(new ApiFeaturestore.FeatureViewCalculationService.FeatureViewCalculationServiceClient(channel)).DeliverAsync;
                break;
        }
        await write(record, token);
    }
}
