using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiFuel = Acme.Fuel.V1;
using ApiPerformance = Acme.Performance.V1;

namespace Acme.Workforce.Ops.AssetsSweeper;

/// <summary>Where PropagateCorporateEventStatus hands each record.</summary>
internal interface IPropagateCorporateEventStatusTarget
{
    Task PushAsync(PropagateCorporateEventStatusRecord record, CancellationToken token);
}

/// <summary>What PropagateCorporateEventStatus keeps of each PreviewCorporateEvent response.</summary>
internal sealed class PropagateCorporateEventStatusRecord
{
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Passes records to acme.performance.v1.FeedbackWorkflowService.</summary>
internal sealed class PropagateCorporateEventStatusFeedbackWorkflowRelay : IPropagateCorporateEventStatusTarget
{
    private readonly ApiPerformance.FeedbackWorkflowService.FeedbackWorkflowServiceClient _client;

    public PropagateCorporateEventStatusFeedbackWorkflowRelay(ApiPerformance.FeedbackWorkflowService.FeedbackWorkflowServiceClient client) => _client = client;

    public async Task PushAsync(PropagateCorporateEventStatusRecord record, CancellationToken token)
    {
        await _client.ResumeFeedbackWorkflowAsync(new ApiPerformance.ResumeFeedbackWorkflowRequest { FeedbackId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.fuel.v1.FuelCardService.</summary>
internal sealed class PropagateCorporateEventStatusFuelCardTarget(ApiFuel.FuelCardService.FuelCardServiceClient client) : IPropagateCorporateEventStatusTarget
{
    public async Task PushAsync(PropagateCorporateEventStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiFuel.GetFuelCardRequest { FuelCardId = record.Status }, token, client.GetFuelCardAsync);
    }
}

/// <summary>Reads acme.corporateevents.v1.CorporateEventValidationService/PreviewCorporateEvent and hands what it keeps to the sink of its kind.</summary>
internal static class PropagateCorporateEventStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiCorporateevents.CorporateEventValidationService.CorporateEventValidationServiceClient(channel);
        var got = await source.PreviewCorporateEventAsync(new ApiCorporateevents.PreviewCorporateEventRequest() { Reason = "primary-propagate-corporate-event-status" }, cancellationToken: token);
        var record = new PropagateCorporateEventStatusRecord
        {
            Status = got.Outcome.Status,
            CorrelationId = got.Result.CorrelationId,
        };
        await ActOnPropagateCorporateEventStatusAsync("feedback-workflow", record, channel, token);
    }

    private static async Task ActOnPropagateCorporateEventStatusAsync(string kind, PropagateCorporateEventStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["feedback-workflow"] = () => new PropagateCorporateEventStatusFeedbackWorkflowRelay(new ApiPerformance.FeedbackWorkflowService.FeedbackWorkflowServiceClient(channel)).PushAsync(record, token),
            ["fuel-card"] = () => new PropagateCorporateEventStatusFuelCardTarget(new ApiFuel.FuelCardService.FuelCardServiceClient(channel)).PushAsync(record, token),
        };
        await actions[kind]();
    }
}
