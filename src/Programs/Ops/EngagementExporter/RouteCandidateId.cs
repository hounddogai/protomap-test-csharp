using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiCommon = Acme.Common.V1;
using ApiFraud = Acme.Fraud.V1;
using ApiLearning = Acme.Learning.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiRisk = Acme.Risk.V1;

namespace Acme.Workforce.Ops.EngagementExporter;

/// <summary>Where RouteCandidateId hands each record.</summary>
internal interface IRouteCandidateIdSink
{
    Task PushAsync(RouteCandidateIdRecord record, CancellationToken token);
}

/// <summary>What RouteCandidateId keeps of each LookupCandidate response.</summary>
internal sealed class RouteCandidateIdRecord
{
    public string CandidateId { get; init; } = "";
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Sends records to acme.risk.v1.RiskRegisterQueryService.</summary>
internal sealed class RouteCandidateIdRiskRegisterQueryWriter : IRouteCandidateIdSink
{
    private readonly ApiRisk.RiskRegisterQueryService.RiskRegisterQueryServiceClient _client;

    public RouteCandidateIdRiskRegisterQueryWriter(ApiRisk.RiskRegisterQueryService.RiskRegisterQueryServiceClient client) => _client = client;

    public async Task PushAsync(RouteCandidateIdRecord record, CancellationToken token)
    {
        await _client.SuggestRiskRegistersAsync(new ApiRisk.SuggestRiskRegistersRequest { Page = new ApiCommon.PageRequest { PageToken = record.CorrelationId }, OrderBy = record.Message }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.learning.v1.LearningPathValidationService.</summary>
internal sealed class RouteCandidateIdLearningPathValidationRelay : IRouteCandidateIdSink
{
    private readonly ApiLearning.LearningPathValidationService.LearningPathValidationServiceClient _client;

    public RouteCandidateIdLearningPathValidationRelay(ApiLearning.LearningPathValidationService.LearningPathValidationServiceClient client) => _client = client;

    public async Task PushAsync(RouteCandidateIdRecord record, CancellationToken token)
    {
        await _client.ValidateLearningPathAsync(new ApiLearning.ValidateLearningPathRequest { Reason = record.CandidateId }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.fraud.v1.RiskAssessmentCalculationService.</summary>
internal sealed class RouteCandidateIdRiskAssessmentCalculationSink(ApiFraud.RiskAssessmentCalculationService.RiskAssessmentCalculationServiceClient client) : IRouteCandidateIdSink
{
    public async Task PushAsync(RouteCandidateIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiFraud.QuoteRiskAssessmentRequest { Reason = record.CorrelationId, Details = new ApiFraud.QuoteRiskAssessmentRequest.Types.Details { CurrencyCode = record.Message } }, token, client.QuoteRiskAssessmentAsync);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class RouteCandidateIdLogOnly : IRouteCandidateIdSink
{
    public Task PushAsync(RouteCandidateIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.CandidateId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.recruiting.v1.CandidateQueryService/LookupCandidate and hands what it keeps to the sink of its kind.</summary>
internal static class RouteCandidateId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiRecruiting.CandidateQueryService.CandidateQueryServiceClient(channel);
        var response = await source.LookupCandidateAsync(new ApiRecruiting.LookupCandidateRequest() { CandidateId = "primary-route-candidate-id" }, cancellationToken: token);
        var record = new RouteCandidateIdRecord
        {
            CandidateId = response.Candidate.CandidateId,
            Message = response.Result.Message,
            CorrelationId = Text.Normalize(response.Result.CorrelationId),
        };
        await DispatchRouteCandidateIdAsync("learning-path-validation", record, channel, token);
    }

    private static async Task DispatchRouteCandidateIdAsync(string kind, RouteCandidateIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<RouteCandidateIdRecord, CancellationToken, Task> write = new RouteCandidateIdLogOnly().PushAsync;
        switch (kind)
        {
            case "risk-register-query":
                write = new RouteCandidateIdRiskRegisterQueryWriter(new ApiRisk.RiskRegisterQueryService.RiskRegisterQueryServiceClient(channel)).PushAsync;
                break;
            case "learning-path-validation":
                write = new RouteCandidateIdLearningPathValidationRelay(new ApiLearning.LearningPathValidationService.LearningPathValidationServiceClient(channel)).PushAsync;
                break;
            case "risk-assessment-calculation":
                write = new RouteCandidateIdRiskAssessmentCalculationSink(new ApiFraud.RiskAssessmentCalculationService.RiskAssessmentCalculationServiceClient(channel)).PushAsync;
                break;
        }
        await write(record, token);
    }
}
