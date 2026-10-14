using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiAnalytics = Acme.Analytics.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDispatch = Acme.Dispatch.V1;
using ApiLearning = Acme.Learning.V1;

namespace Acme.Workforce.Ops.AbsenceRelay;

/// <summary>Where PropagateVenueCode hands each record.</summary>
internal interface IPropagateVenueCodeTarget
{
    Task WriteAsync(PropagateVenueCodeRecord record, CancellationToken token);
}

/// <summary>What PropagateVenueCode keeps of each ScoreVenue response.</summary>
internal sealed class PropagateVenueCodeRecord
{
    public string Code { get; init; } = "";
    public string Status { get; init; } = "";
    public string CurrencyCode { get; init; } = "";
    public string Name { get; init; } = "";
}

/// <summary>Passes records to acme.learning.v1.LearningPathCalculationService.</summary>
internal sealed class PropagateVenueCodeLearningPathCalculationTarget : IPropagateVenueCodeTarget
{
    private readonly ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient _client;

    public PropagateVenueCodeLearningPathCalculationTarget(ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient client) => _client = client;

    public async Task WriteAsync(PropagateVenueCodeRecord record, CancellationToken token)
    {
        await _client.CalculateLearningPathAsync(new ApiLearning.CalculateLearningPathRequest { Details = new ApiLearning.CalculateLearningPathRequest.Types.Details { CurrencyCode = record.CurrencyCode } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.dispatch.v1.DispatchOrderService.</summary>
internal sealed class PropagateVenueCodeDispatchOrderRelay : IPropagateVenueCodeTarget
{
    private readonly ApiDispatch.DispatchOrderService.DispatchOrderServiceClient _client;

    public PropagateVenueCodeDispatchOrderRelay(ApiDispatch.DispatchOrderService.DispatchOrderServiceClient client) => _client = client;

    public async Task WriteAsync(PropagateVenueCodeRecord record, CancellationToken token)
    {
        await _client.GetDispatchOrderAsync(new ApiDispatch.GetDispatchOrderRequest { DispatchOrderId = record.Name }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.analytics.v1.AnalyticsEventService.</summary>
internal sealed class PropagateVenueCodeAnalyticsEventRelay(ApiAnalytics.AnalyticsEventService.AnalyticsEventServiceClient client) : IPropagateVenueCodeTarget
{
    public async Task WriteAsync(PropagateVenueCodeRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiAnalytics.UpdateAnalyticsEventRequest { AnalyticsEvent = new ApiAnalytics.AnalyticsEvent { CorrelationId = record.Code } }, token, client.UpdateAnalyticsEventAsync);
    }
}

/// <summary>Reads acme.corporateevents.v1.VenueValidationService/ScoreVenue and hands what it keeps to the sink of its kind.</summary>
internal static class PropagateVenueCode
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiCorporateevents.VenueValidationService.VenueValidationServiceClient(channel);
        var response = await source.ScoreVenueAsync(new ApiCorporateevents.ScoreVenueRequest() { VenueId = "current-propagate-venue-code" }, cancellationToken: token);
        var record = new PropagateVenueCodeRecord
        {
            Code = response.Venue.Code,
            Status = response.Result.Status,
            CurrencyCode = Text.Normalize(response.Breakdown.CurrencyCode),
            Name = response.Venue.Name,
        };
        var sinks = new Dictionary<string, IPropagateVenueCodeTarget>
        {
            ["learning-path-calculation"] = new PropagateVenueCodeLearningPathCalculationTarget(new ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient(channel)),
            ["dispatch-order"] = new PropagateVenueCodeDispatchOrderRelay(new ApiDispatch.DispatchOrderService.DispatchOrderServiceClient(channel)),
            ["analytics-event"] = new PropagateVenueCodeAnalyticsEventRelay(new ApiAnalytics.AnalyticsEventService.AnalyticsEventServiceClient(channel)),
        };
        await RoutePropagateVenueCodeAsync("dispatch-order", record, sinks, token);
    }

    private static async Task RoutePropagateVenueCodeAsync(string kind, PropagateVenueCodeRecord record, IReadOnlyDictionary<string, IPropagateVenueCodeTarget> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.WriteAsync(record, token);
    }
}
