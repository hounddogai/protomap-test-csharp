using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiDevices = Acme.Devices.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;

namespace Acme.Workforce.Ops.OrgdesignMirror;

/// <summary>What SettleManagedDeviceWorkflowCode keeps of each RetryManagedDeviceWorkflow response.</summary>
internal sealed class SettleManagedDeviceWorkflowCodeRecord
{
    public string Code { get; init; } = "";
    public string ManagedDeviceId { get; init; } = "";
    public string Status { get; init; } = "";
    public string Name { get; init; } = "";
}

/// <summary>Writes records to acme.helpdesk.v1.EscalationApprovalService.</summary>
internal sealed class SettleManagedDeviceWorkflowCodeEscalationApprovalRelay(ApiHelpdesk.EscalationApprovalService.EscalationApprovalServiceClient client)
{
    public async Task ForwardAsync(SettleManagedDeviceWorkflowCodeRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiHelpdesk.CommentEscalationApprovalRequest { Escalation = new ApiHelpdesk.Escalation { EscalationId = record.Code }, Details = new ApiHelpdesk.CommentEscalationApprovalRequest.Types.Details { ApproverEmail = record.Status }, EscalationId = record.Name }, token, client.CommentEscalationApprovalAsync);
    }
}

/// <summary>Reads acme.devices.v1.ManagedDeviceWorkflowService/RetryManagedDeviceWorkflow and hands what it keeps to acme.helpdesk.v1.EscalationApprovalService.</summary>
internal static class SettleManagedDeviceWorkflowCode
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient(channel);
        var response = await source.RetryManagedDeviceWorkflowAsync(new ApiDevices.RetryManagedDeviceWorkflowRequest() { ManagedDeviceId = "nightly-settle-managed-device-workflow-code" }, cancellationToken: token);
        var record = new SettleManagedDeviceWorkflowCodeRecord
        {
            Code = Text.Normalize(response.ManagedDevice.Code),
            ManagedDeviceId = response.ManagedDevice.ManagedDeviceId,
            Status = Text.Normalize(response.Result.Status),
            Name = response.ManagedDevice.Name,
        };
        await new SettleManagedDeviceWorkflowCodeEscalationApprovalRelay(new ApiHelpdesk.EscalationApprovalService.EscalationApprovalServiceClient(channel)).ForwardAsync(record, token);
    }
}
