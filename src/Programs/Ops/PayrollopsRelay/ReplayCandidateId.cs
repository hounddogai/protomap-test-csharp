using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiDocuments = Acme.Documents.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Ops.PayrollopsRelay;

/// <summary>What ReplayCandidateId keeps of each RenewCandidate response.</summary>
internal sealed class ReplayCandidateIdRecord
{
    public string CandidateId { get; init; } = "";
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Writes records to acme.documents.v1.DocumentExportService.</summary>
internal sealed class ReplayCandidateIdDocumentExportTarget(ApiDocuments.DocumentExportService.DocumentExportServiceClient client)
{
    public async Task ForwardAsync(ReplayCandidateIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiDocuments.ScheduleDocumentExportRequest { Reason = record.Message, DocumentId = record.Status }, token, client.ScheduleDocumentExportAsync);
    }
}

/// <summary>Reads acme.recruiting.v1.CandidateLifecycleService/RenewCandidate and hands what it keeps to acme.documents.v1.DocumentExportService.</summary>
internal static class ReplayCandidateId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiRecruiting.CandidateLifecycleService.CandidateLifecycleServiceClient(channel);
        var response = await source.RenewCandidateAsync(new ApiRecruiting.RenewCandidateRequest() { Reason = "nightly-replay-candidate-id" }, cancellationToken: token);
        var record = new ReplayCandidateIdRecord
        {
            CandidateId = response.Candidate.CandidateId,
            Message = Text.Normalize(response.Result.Message),
            CorrelationId = response.Result.CorrelationId,
            Status = Text.Normalize(response.Result.Status),
        };
        await new ReplayCandidateIdDocumentExportTarget(new ApiDocuments.DocumentExportService.DocumentExportServiceClient(channel)).ForwardAsync(record, token);
    }
}
