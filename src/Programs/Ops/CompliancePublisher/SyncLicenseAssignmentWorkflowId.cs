using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Ops.CompliancePublisher;

/// <summary>What SyncLicenseAssignmentWorkflowId keeps of each SignalLicenseAssignmentWorkflow response.</summary>
internal sealed class SyncLicenseAssignmentWorkflowIdRecord
{
    public string LicenseAssignmentId { get; init; } = "";
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
    public string ResultMessage { get; init; } = "";
}

/// <summary>Passes records to acme.recruiting.v1.InterviewWorkflowService.</summary>
internal sealed class SyncLicenseAssignmentWorkflowIdInterviewWorkflowSink
{
    private readonly ApiRecruiting.InterviewWorkflowService.InterviewWorkflowServiceClient _client;

    public SyncLicenseAssignmentWorkflowIdInterviewWorkflowSink(ApiRecruiting.InterviewWorkflowService.InterviewWorkflowServiceClient client) => _client = client;

    public async Task ApplyAsync(SyncLicenseAssignmentWorkflowIdRecord record, CancellationToken token)
    {
        await _client.StartInterviewWorkflowAsync(new ApiRecruiting.StartInterviewWorkflowRequest { Interview = new ApiRecruiting.Interview { Title = record.Message }, Details = new ApiRecruiting.StartInterviewWorkflowRequest.Types.Details { Step = record.LicenseAssignmentId } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.licensing.v1.LicenseAssignmentWorkflowService/SignalLicenseAssignmentWorkflow and hands what it keeps to acme.recruiting.v1.InterviewWorkflowService.</summary>
internal static class SyncLicenseAssignmentWorkflowId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiLicensing.LicenseAssignmentWorkflowService.LicenseAssignmentWorkflowServiceClient(channel);
        var got = await source.SignalLicenseAssignmentWorkflowAsync(new ApiLicensing.SignalLicenseAssignmentWorkflowRequest() { Reason = "current-sync-license-assignment-workflow-id" }, cancellationToken: token);
        var record = new SyncLicenseAssignmentWorkflowIdRecord
        {
            LicenseAssignmentId = got.LicenseAssignment.LicenseAssignmentId,
            Message = got.Outcome.Message,
            Status = got.Result.Status,
            ResultMessage = Text.Normalize(got.Result.Message),
        };
        await new SyncLicenseAssignmentWorkflowIdInterviewWorkflowSink(new ApiRecruiting.InterviewWorkflowService.InterviewWorkflowServiceClient(channel)).ApplyAsync(record, token);
    }
}
