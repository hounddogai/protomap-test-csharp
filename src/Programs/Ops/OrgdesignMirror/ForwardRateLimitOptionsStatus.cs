using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiPatients = Acme.Patients.V1;
using ApiRatelimiting = Acme.Ratelimiting.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiSurveys = Acme.Surveys.V1;

namespace Acme.Workforce.Ops.OrgdesignMirror;

/// <summary>Where ForwardRateLimitOptionsStatus hands each record.</summary>
internal interface IForwardRateLimitOptionsStatusSink
{
    Task PushAsync(ForwardRateLimitOptionsStatusRecord record, CancellationToken token);
}

/// <summary>What ForwardRateLimitOptionsStatus keeps of each CompareRateLimitOptions response.</summary>
internal sealed class ForwardRateLimitOptionsStatusRecord
{
    public string Status { get; init; } = "";
    public string Name { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Hands records to acme.patients.v1.PatientService.</summary>
internal sealed class ForwardRateLimitOptionsStatusPatientTarget : IForwardRateLimitOptionsStatusSink
{
    private readonly ApiPatients.PatientService.PatientServiceClient _client;

    public ForwardRateLimitOptionsStatusPatientTarget(ApiPatients.PatientService.PatientServiceClient client) => _client = client;

    public async Task PushAsync(ForwardRateLimitOptionsStatusRecord record, CancellationToken token)
    {
        await _client.GetPatientAsync(new ApiPatients.GetPatientRequest { PatientId = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.scheduling.v1.AvailabilityWindowWorkflowService.</summary>
internal sealed class ForwardRateLimitOptionsStatusAvailabilityWindowWorkflowWriter(ApiScheduling.AvailabilityWindowWorkflowService.AvailabilityWindowWorkflowServiceClient client) : IForwardRateLimitOptionsStatusSink
{
    public async Task PushAsync(ForwardRateLimitOptionsStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiScheduling.StartAvailabilityWindowWorkflowRequest { Reason = record.Status, AvailabilityWindow = new ApiScheduling.AvailabilityWindow { Title = record.Name } }, token, client.StartAvailabilityWindowWorkflowAsync);
    }
}

/// <summary>Writes records to acme.surveys.v1.SurveyService.</summary>
internal sealed class ForwardRateLimitOptionsStatusSurveyTarget(ApiSurveys.SurveyService.SurveyServiceClient client) : IForwardRateLimitOptionsStatusSink
{
    public async Task PushAsync(ForwardRateLimitOptionsStatusRecord record, CancellationToken token)
    {
        await client.CreateSurveyAsync(new ApiSurveys.CreateSurveyRequest { Survey = new ApiSurveys.Survey { Number = record.CorrelationId, SurveyId = record.Name }, RequestId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Reads acme.ratelimiting.v1.RateLimitCalculationService/CompareRateLimitOptions and hands what it keeps to the sink of its kind.</summary>
internal static class ForwardRateLimitOptionsStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiRatelimiting.RateLimitCalculationService.RateLimitCalculationServiceClient(channel);
        var response = await source.CompareRateLimitOptionsAsync(new ApiRatelimiting.CompareRateLimitOptionsRequest() { RateLimitId = "primary-forward-rate-limit-options-status" }, cancellationToken: token);
        var record = new ForwardRateLimitOptionsStatusRecord
        {
            Status = response.Result.Status,
            Name = response.RateLimit.Name,
            CorrelationId = response.Result.CorrelationId,
        };
        var sinks = new Dictionary<string, IForwardRateLimitOptionsStatusSink>
        {
            ["patient"] = new ForwardRateLimitOptionsStatusPatientTarget(new ApiPatients.PatientService.PatientServiceClient(channel)),
            ["availability-window-workflow"] = new ForwardRateLimitOptionsStatusAvailabilityWindowWorkflowWriter(new ApiScheduling.AvailabilityWindowWorkflowService.AvailabilityWindowWorkflowServiceClient(channel)),
            ["survey"] = new ForwardRateLimitOptionsStatusSurveyTarget(new ApiSurveys.SurveyService.SurveyServiceClient(channel)),
        };
        var target = Environment.GetEnvironmentVariable("FORWARD_RATE_LIMIT_OPTIONS_STATUS_TARGET") ?? "";
        if (sinks.ContainsKey(target))
            await sinks[target].PushAsync(record, token);
    }
}
