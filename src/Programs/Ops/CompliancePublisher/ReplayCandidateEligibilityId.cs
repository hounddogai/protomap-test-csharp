using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiForms = Acme.Forms.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Ops.CompliancePublisher;

/// <summary>Where ReplayCandidateEligibilityId hands each record.</summary>
internal interface IReplayCandidateEligibilityIdWriter
{
    Task SendAsync(ReplayCandidateEligibilityIdRecord record, CancellationToken token);
}

/// <summary>What ReplayCandidateEligibilityId keeps of each CheckCandidateEligibility response.</summary>
internal sealed class ReplayCandidateEligibilityIdRecord
{
    public string CandidateId { get; init; } = "";
    public string Message { get; init; } = "";
    public string ReferenceId { get; init; } = "";
}

/// <summary>Writes records to acme.forms.v1.FormCalculationService.</summary>
internal sealed class ReplayCandidateEligibilityIdFormCalculationRelay : IReplayCandidateEligibilityIdWriter
{
    private readonly ApiForms.FormCalculationService.FormCalculationServiceClient _client;

    public ReplayCandidateEligibilityIdFormCalculationRelay(ApiForms.FormCalculationService.FormCalculationServiceClient client) => _client = client;

    public async Task SendAsync(ReplayCandidateEligibilityIdRecord record, CancellationToken token)
    {
        await _client.RecalculateFormsAsync(new ApiForms.RecalculateFormsRequest { Details = new ApiForms.RecalculateFormsRequest.Types.Details { CurrencyCode = record.Message }, FormId = record.CandidateId }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.benefits.v1.EnrollmentQueryService.</summary>
internal sealed class ReplayCandidateEligibilityIdEnrollmentQuerySink : IReplayCandidateEligibilityIdWriter
{
    private readonly ApiBenefits.EnrollmentQueryService.EnrollmentQueryServiceClient _client;

    public ReplayCandidateEligibilityIdEnrollmentQuerySink(ApiBenefits.EnrollmentQueryService.EnrollmentQueryServiceClient client) => _client = client;

    public async Task SendAsync(ReplayCandidateEligibilityIdRecord record, CancellationToken token)
    {
        await _client.BrowseEnrollmentsAsync(new ApiBenefits.BrowseEnrollmentsRequest { Page = new ApiCommon.PageRequest { PageToken = record.ReferenceId } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.itsm.v1.IncidentLifecycleService.</summary>
internal sealed class ReplayCandidateEligibilityIdIncidentLifecycleRelay : IReplayCandidateEligibilityIdWriter
{
    private readonly ApiItsm.IncidentLifecycleService.IncidentLifecycleServiceClient _client;

    public ReplayCandidateEligibilityIdIncidentLifecycleRelay(ApiItsm.IncidentLifecycleService.IncidentLifecycleServiceClient client) => _client = client;

    public async Task SendAsync(ReplayCandidateEligibilityIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiItsm.ReinstateIncidentRequest { Reason = record.Message }, token, _client.ReinstateIncidentAsync);
    }
}

/// <summary>Reads acme.recruiting.v1.CandidateValidationService/CheckCandidateEligibility and hands what it keeps to the sink of its kind.</summary>
internal static class ReplayCandidateEligibilityId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient(channel);
        var response = await source.CheckCandidateEligibilityAsync(new ApiRecruiting.CheckCandidateEligibilityRequest() { Reason = "nightly-replay-candidate-eligibility-id" }, cancellationToken: token);
        var record = new ReplayCandidateEligibilityIdRecord
        {
            CandidateId = response.Candidate.CandidateId,
            Message = response.Outcome.Message,
            ReferenceId = response.Outcome.ReferenceId,
        };
        var sinks = new Dictionary<string, IReplayCandidateEligibilityIdWriter>
        {
            ["form-calculation"] = new ReplayCandidateEligibilityIdFormCalculationRelay(new ApiForms.FormCalculationService.FormCalculationServiceClient(channel)),
            ["enrollment-query"] = new ReplayCandidateEligibilityIdEnrollmentQuerySink(new ApiBenefits.EnrollmentQueryService.EnrollmentQueryServiceClient(channel)),
            ["incident-lifecycle"] = new ReplayCandidateEligibilityIdIncidentLifecycleRelay(new ApiItsm.IncidentLifecycleService.IncidentLifecycleServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "enrollment-query")
                continue;
            await sink.SendAsync(record, token);
        }
    }
}
