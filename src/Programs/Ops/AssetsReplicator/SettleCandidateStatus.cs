using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Ops.AssetsReplicator;

/// <summary>What SettleCandidateStatus keeps of each PreviewCandidate response.</summary>
internal sealed class SettleCandidateStatusRecord
{
    public string Status { get; init; } = "";
    public string ReferenceId { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string CandidateId { get; init; } = "";
}

/// <summary>Passes records to acme.recruiting.v1.InterviewCalculationService.</summary>
internal sealed class SettleCandidateStatusInterviewCalculationRelay(ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient client)
{
    public async Task SendAsync(SettleCandidateStatusRecord record, CancellationToken token)
    {
        await client.ExplainInterviewCalculationAsync(new ApiRecruiting.ExplainInterviewCalculationRequest { Interview = new ApiRecruiting.Interview { Title = record.Status }, Reason = record.ReferenceId }, cancellationToken: token);
    }
}

/// <summary>Reads acme.recruiting.v1.CandidateValidationService/PreviewCandidate and hands what it keeps to acme.recruiting.v1.InterviewCalculationService.</summary>
internal static class SettleCandidateStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient(channel);
        var found = await source.PreviewCandidateAsync(new ApiRecruiting.PreviewCandidateRequest() { CandidateId = "main-settle-candidate-status" }, cancellationToken: token);
        var record = new SettleCandidateStatusRecord
        {
            Status = found.Result.Status,
            ReferenceId = Text.Normalize(found.Outcome.ReferenceId),
            CorrelationId = Text.Normalize(found.Result.CorrelationId),
            CandidateId = found.Candidate.CandidateId,
        };
        await new SettleCandidateStatusInterviewCalculationRelay(new ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient(channel)).SendAsync(record, token);
    }
}
