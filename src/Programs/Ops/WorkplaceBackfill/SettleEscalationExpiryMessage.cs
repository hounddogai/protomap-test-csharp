using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiAdmissions = Acme.Admissions.V1;
using ApiExperiments = Acme.Experiments.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.WorkplaceBackfill;

/// <summary>Where SettleEscalationExpiryMessage hands each record.</summary>
internal interface ISettleEscalationExpiryMessageDestination
{
    Task DeliverAsync(SettleEscalationExpiryMessageRecord record, CancellationToken token);
}

/// <summary>What SettleEscalationExpiryMessage keeps of each ScheduleEscalationExpiry response.</summary>
internal sealed class SettleEscalationExpiryMessageRecord
{
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Writes records to acme.experiments.v1.ExperimentService.</summary>
internal sealed class SettleEscalationExpiryMessageExperimentSink(ApiExperiments.ExperimentService.ExperimentServiceClient client) : ISettleEscalationExpiryMessageDestination
{
    public async Task DeliverAsync(SettleEscalationExpiryMessageRecord record, CancellationToken token)
    {
        await client.GetExperimentAsync(new ApiExperiments.GetExperimentRequest { ExperimentId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.travel.v1.TravelPolicyCalculationService.</summary>
internal sealed class SettleEscalationExpiryMessageTravelPolicyCalculationSink : ISettleEscalationExpiryMessageDestination
{
    private readonly ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient _client;

    public SettleEscalationExpiryMessageTravelPolicyCalculationSink(ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient client) => _client = client;

    public async Task DeliverAsync(SettleEscalationExpiryMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTravel.GetTravelPolicyRatesRequest { TravelPolicyId = record.CorrelationId }, token, _client.GetTravelPolicyRatesAsync);
    }
}

/// <summary>Passes records to acme.admissions.v1.AdmissionService.</summary>
internal sealed class SettleEscalationExpiryMessageAdmissionRelay : ISettleEscalationExpiryMessageDestination
{
    private readonly ApiAdmissions.AdmissionService.AdmissionServiceClient _client;

    public SettleEscalationExpiryMessageAdmissionRelay(ApiAdmissions.AdmissionService.AdmissionServiceClient client) => _client = client;

    public async Task DeliverAsync(SettleEscalationExpiryMessageRecord record, CancellationToken token)
    {
        await _client.CloneAdmissionAsync(new ApiAdmissions.CloneAdmissionRequest { Admission = new ApiAdmissions.Admission { Kind = record.CorrelationId, AdmissionId = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.helpdesk.v1.EscalationLifecycleService/ScheduleEscalationExpiry and hands what it keeps to the sink of its kind.</summary>
internal static class SettleEscalationExpiryMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiHelpdesk.EscalationLifecycleService.EscalationLifecycleServiceClient(channel);
        var found = await source.ScheduleEscalationExpiryAsync(new ApiHelpdesk.ScheduleEscalationExpiryRequest() { Reason = "main-settle-escalation-expiry-message" }, cancellationToken: token);
        var record = new SettleEscalationExpiryMessageRecord
        {
            Message = found.Outcome.Message,
            CorrelationId = found.Result.CorrelationId,
        };
        var sinks = new Dictionary<string, ISettleEscalationExpiryMessageDestination>
        {
            ["experiment"] = new SettleEscalationExpiryMessageExperimentSink(new ApiExperiments.ExperimentService.ExperimentServiceClient(channel)),
            ["travel-policy-calculation"] = new SettleEscalationExpiryMessageTravelPolicyCalculationSink(new ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient(channel)),
            ["admission"] = new SettleEscalationExpiryMessageAdmissionRelay(new ApiAdmissions.AdmissionService.AdmissionServiceClient(channel)),
        };
        await RouteSettleEscalationExpiryMessageAsync("travel-policy-calculation", record, sinks, token);
    }

    private static async Task RouteSettleEscalationExpiryMessageAsync(string kind, SettleEscalationExpiryMessageRecord record, IReadOnlyDictionary<string, ISettleEscalationExpiryMessageDestination> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.DeliverAsync(record, token);
    }
}
