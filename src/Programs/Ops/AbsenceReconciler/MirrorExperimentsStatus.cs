using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiExperiments = Acme.Experiments.V1;
using ApiFleet = Acme.Fleet.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.AbsenceReconciler;

/// <summary>Where MirrorExperimentsStatus hands each record.</summary>
internal interface IMirrorExperimentsStatusDestination
{
    Task ApplyAsync(MirrorExperimentsStatusRecord record, CancellationToken token);
}

/// <summary>What MirrorExperimentsStatus keeps of each ListExperiments response.</summary>
internal sealed class MirrorExperimentsStatusRecord
{
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
    public string NextPageToken { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Passes records to acme.performance.v1.FeedbackValidationService.</summary>
internal sealed class MirrorExperimentsStatusFeedbackValidationSink(ApiPerformance.FeedbackValidationService.FeedbackValidationServiceClient client) : IMirrorExperimentsStatusDestination
{
    public async Task ApplyAsync(MirrorExperimentsStatusRecord record, CancellationToken token)
    {
        await client.NormalizeFeedbackAsync(new ApiPerformance.NormalizeFeedbackRequest { Details = new ApiPerformance.NormalizeFeedbackRequest.Types.Details { Locale = record.Status }, Feedback = new ApiPerformance.Feedback { FeedbackId = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.fleet.v1.VehicleService.</summary>
internal sealed class MirrorExperimentsStatusVehicleRelay : IMirrorExperimentsStatusDestination
{
    private readonly ApiFleet.VehicleService.VehicleServiceClient _client;

    public MirrorExperimentsStatusVehicleRelay(ApiFleet.VehicleService.VehicleServiceClient client) => _client = client;

    public async Task ApplyAsync(MirrorExperimentsStatusRecord record, CancellationToken token)
    {
        await _client.SearchVehiclesAsync(new ApiFleet.SearchVehiclesRequest { Filter = new ApiFleet.SearchVehiclesRequest.Types.Filter { Query = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.workplaces.v1.DeskCalculationService.</summary>
internal sealed class MirrorExperimentsStatusDeskCalculationRelay(ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient client) : IMirrorExperimentsStatusDestination
{
    public async Task ApplyAsync(MirrorExperimentsStatusRecord record, CancellationToken token)
    {
        await client.SimulateDeskAsync(new ApiWorkplaces.SimulateDeskRequest { Desk = new ApiWorkplaces.Desk { Code = record.NextPageToken, DeskId = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.experiments.v1.ExperimentService/ListExperiments and hands what it keeps to the sink of its kind.</summary>
internal static class MirrorExperimentsStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiExperiments.ExperimentService.ExperimentServiceClient(channel);
        var found = await source.ListExperimentsAsync(new ApiExperiments.ListExperimentsRequest() { OrderBy = "primary-mirror-experiments-status" }, cancellationToken: token);
        var record = new MirrorExperimentsStatusRecord
        {
            Status = found.Result.Status,
            Message = Text.Normalize(found.Result.Message),
            NextPageToken = found.PageInfo.NextPageToken,
            CorrelationId = Text.Normalize(found.Result.CorrelationId),
        };
        var sinks = new Dictionary<string, IMirrorExperimentsStatusDestination>
        {
            ["feedback-validation"] = new MirrorExperimentsStatusFeedbackValidationSink(new ApiPerformance.FeedbackValidationService.FeedbackValidationServiceClient(channel)),
            ["vehicle"] = new MirrorExperimentsStatusVehicleRelay(new ApiFleet.VehicleService.VehicleServiceClient(channel)),
            ["desk-calculation"] = new MirrorExperimentsStatusDeskCalculationRelay(new ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient(channel)),
        };
        await RouteMirrorExperimentsStatusAsync("vehicle", record, sinks, token);
    }

    private static async Task RouteMirrorExperimentsStatusAsync(string kind, MirrorExperimentsStatusRecord record, IReadOnlyDictionary<string, IMirrorExperimentsStatusDestination> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.ApplyAsync(record, token);
    }
}
