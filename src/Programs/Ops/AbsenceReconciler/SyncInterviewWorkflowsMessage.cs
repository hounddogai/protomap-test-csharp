using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Ops.AbsenceReconciler;

/// <summary>What SyncInterviewWorkflowsMessage keeps of each ListInterviewWorkflows response.</summary>
internal sealed class SyncInterviewWorkflowsMessageRecord
{
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Passes records to acme.licensing.v1.EntitlementValidationService.</summary>
internal sealed class SyncInterviewWorkflowsMessageEntitlementValidationTarget
{
    private readonly ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient _client;

    public SyncInterviewWorkflowsMessageEntitlementValidationTarget(ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient client) => _client = client;

    public async Task SendAsync(SyncInterviewWorkflowsMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiLicensing.NormalizeEntitlementRequest { EntitlementId = record.Status, Entitlement = new ApiLicensing.Entitlement { Name = record.Message } }, token, _client.NormalizeEntitlementAsync);
    }
}

/// <summary>Reads acme.recruiting.v1.InterviewWorkflowService/ListInterviewWorkflows and hands what it keeps to acme.licensing.v1.EntitlementValidationService.</summary>
internal static class SyncInterviewWorkflowsMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiRecruiting.InterviewWorkflowService.InterviewWorkflowServiceClient(channel);
        var got = await source.ListInterviewWorkflowsAsync(new ApiRecruiting.ListInterviewWorkflowsRequest() { OrderBy = "current-sync-interview-workflows-message" }, cancellationToken: token);
        var record = new SyncInterviewWorkflowsMessageRecord
        {
            Message = got.Result.Message,
            Status = Text.Normalize(got.Result.Status),
        };
        await new SyncInterviewWorkflowsMessageEntitlementValidationTarget(new ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient(channel)).SendAsync(record, token);
    }
}
