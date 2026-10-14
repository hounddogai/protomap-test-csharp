using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiDevices = Acme.Devices.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.PayrollopsRelay;

/// <summary>Where PropagateDevicePoliciesMessage hands each record.</summary>
internal interface IPropagateDevicePoliciesMessageSink
{
    Task ForwardAsync(PropagateDevicePoliciesMessageRecord record, CancellationToken token);
}

/// <summary>What PropagateDevicePoliciesMessage keeps of each BrowseDevicePolicies response.</summary>
internal sealed class PropagateDevicePoliciesMessageRecord
{
    public string Message { get; init; } = "";
    public string NextPageToken { get; init; } = "";
}

/// <summary>Passes records to acme.licensing.v1.SoftwareLicenseApprovalService.</summary>
internal sealed class PropagateDevicePoliciesMessageSoftwareLicenseApprovalWriter : IPropagateDevicePoliciesMessageSink
{
    private readonly ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient _client;

    public PropagateDevicePoliciesMessageSoftwareLicenseApprovalWriter(ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient client) => _client = client;

    public async Task ForwardAsync(PropagateDevicePoliciesMessageRecord record, CancellationToken token)
    {
        await _client.RejectSoftwareLicenseAsync(new ApiLicensing.RejectSoftwareLicenseRequest { SoftwareLicenseId = record.NextPageToken, Details = new ApiLicensing.RejectSoftwareLicenseRequest.Types.Details { ApproverEmail = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.workplaces.v1.VisitorPassLifecycleService.</summary>
internal sealed class PropagateDevicePoliciesMessageVisitorPassLifecycleRelay : IPropagateDevicePoliciesMessageSink
{
    private readonly ApiWorkplaces.VisitorPassLifecycleService.VisitorPassLifecycleServiceClient _client;

    public PropagateDevicePoliciesMessageVisitorPassLifecycleRelay(ApiWorkplaces.VisitorPassLifecycleService.VisitorPassLifecycleServiceClient client) => _client = client;

    public async Task ForwardAsync(PropagateDevicePoliciesMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiWorkplaces.RenewVisitorPassRequest { Reason = record.Message, VisitorPassId = record.NextPageToken }, token, _client.RenewVisitorPassAsync);
    }
}

/// <summary>Reads acme.devices.v1.DevicePolicyQueryService/BrowseDevicePolicies and hands what it keeps to the sink of its kind.</summary>
internal static class PropagateDevicePoliciesMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDevices.DevicePolicyQueryService.DevicePolicyQueryServiceClient(channel);
        var reply = await source.BrowseDevicePoliciesAsync(new ApiDevices.BrowseDevicePoliciesRequest() { OrderBy = "default-propagate-device-policies-message" }, cancellationToken: token);
        var record = new PropagateDevicePoliciesMessageRecord
        {
            Message = reply.Result.Message,
            NextPageToken = reply.PageInfo.NextPageToken,
        };
        await ActOnPropagateDevicePoliciesMessageAsync("visitor-pass-lifecycle", record, channel, token);
    }

    private static async Task ActOnPropagateDevicePoliciesMessageAsync(string kind, PropagateDevicePoliciesMessageRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["software-license-approval"] = () => new PropagateDevicePoliciesMessageSoftwareLicenseApprovalWriter(new ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient(channel)).ForwardAsync(record, token),
            ["visitor-pass-lifecycle"] = () => new PropagateDevicePoliciesMessageVisitorPassLifecycleRelay(new ApiWorkplaces.VisitorPassLifecycleService.VisitorPassLifecycleServiceClient(channel)).ForwardAsync(record, token),
        };
        await actions[kind]();
    }
}
