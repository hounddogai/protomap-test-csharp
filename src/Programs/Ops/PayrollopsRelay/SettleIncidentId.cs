using Grpc.Net.Client;
using ApiConcessions = Acme.Concessions.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiRatelimiting = Acme.Ratelimiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.PayrollopsRelay;

/// <summary>Where SettleIncidentId hands each record.</summary>
internal interface ISettleIncidentIdSink
{
    Task PushAsync(SettleIncidentIdRecord record, CancellationToken token);
}

/// <summary>What SettleIncidentId keeps of each TerminateIncident response.</summary>
internal sealed class SettleIncidentIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Sends records to acme.concessions.v1.ConcessionStandCalculationService.</summary>
internal sealed class SettleIncidentIdConcessionStandCalculationSink : ISettleIncidentIdSink
{
    private readonly ApiConcessions.ConcessionStandCalculationService.ConcessionStandCalculationServiceClient _client;

    public SettleIncidentIdConcessionStandCalculationSink(ApiConcessions.ConcessionStandCalculationService.ConcessionStandCalculationServiceClient client) => _client = client;

    public async Task PushAsync(SettleIncidentIdRecord record, CancellationToken token)
    {
        await _client.ExplainConcessionStandCalculationAsync(new ApiConcessions.ExplainConcessionStandCalculationRequest { Details = new ApiConcessions.ExplainConcessionStandCalculationRequest.Types.Details { Locale = record.Message }, ConcessionStandId = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.ratelimiting.v1.RateLimitCalculationService.</summary>
internal sealed class SettleIncidentIdRateLimitCalculationRelay : ISettleIncidentIdSink
{
    private readonly ApiRatelimiting.RateLimitCalculationService.RateLimitCalculationServiceClient _client;

    public SettleIncidentIdRateLimitCalculationRelay(ApiRatelimiting.RateLimitCalculationService.RateLimitCalculationServiceClient client) => _client = client;

    public async Task PushAsync(SettleIncidentIdRecord record, CancellationToken token)
    {
        await _client.GetRateLimitRatesAsync(new ApiRatelimiting.GetRateLimitRatesRequest { RateLimitId = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.workplaces.v1.VisitorPassValidationService.</summary>
internal sealed class SettleIncidentIdVisitorPassValidationRelay : ISettleIncidentIdSink
{
    private readonly ApiWorkplaces.VisitorPassValidationService.VisitorPassValidationServiceClient _client;

    public SettleIncidentIdVisitorPassValidationRelay(ApiWorkplaces.VisitorPassValidationService.VisitorPassValidationServiceClient client) => _client = client;

    public async Task PushAsync(SettleIncidentIdRecord record, CancellationToken token)
    {
        await _client.ExplainVisitorPassDecisionAsync(new ApiWorkplaces.ExplainVisitorPassDecisionRequest { VisitorPassId = record.CorrelationId, VisitorPass = new ApiWorkplaces.VisitorPass { VisitorPassId = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.itsm.v1.IncidentLifecycleService/TerminateIncident and hands what it keeps to the sink of its kind.</summary>
internal static class SettleIncidentId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiItsm.IncidentLifecycleService.IncidentLifecycleServiceClient(channel);
        var response = await source.TerminateIncidentAsync(new ApiItsm.TerminateIncidentRequest() { Reason = "primary-settle-incident-id" }, cancellationToken: token);
        var record = new SettleIncidentIdRecord
        {
            CorrelationId = response.Result.CorrelationId,
            Message = response.Outcome.Message,
        };
        var sinks = new Dictionary<string, ISettleIncidentIdSink>
        {
            ["concession-stand-calculation"] = new SettleIncidentIdConcessionStandCalculationSink(new ApiConcessions.ConcessionStandCalculationService.ConcessionStandCalculationServiceClient(channel)),
            ["rate-limit-calculation"] = new SettleIncidentIdRateLimitCalculationRelay(new ApiRatelimiting.RateLimitCalculationService.RateLimitCalculationServiceClient(channel)),
            ["visitor-pass-validation"] = new SettleIncidentIdVisitorPassValidationRelay(new ApiWorkplaces.VisitorPassValidationService.VisitorPassValidationServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "rate-limit-calculation")
                continue;
            await sink.PushAsync(record, token);
        }
    }
}
