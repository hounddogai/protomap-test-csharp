using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Ops.TalentSync;

/// <summary>Where ForwardSlaPolicyName hands each record.</summary>
internal interface IForwardSlaPolicyNameTarget
{
    Task PushAsync(ForwardSlaPolicyNameRecord record, CancellationToken token);
}

/// <summary>What ForwardSlaPolicyName keeps of each ValidateSlaPolicy response.</summary>
internal sealed class ForwardSlaPolicyNameRecord
{
    public string Name { get; init; } = "";
    public string ReferenceId { get; init; } = "";
    public string SlaPolicyId { get; init; } = "";
}

/// <summary>Hands records to acme.licensing.v1.LicenseAssignmentApprovalService.</summary>
internal sealed class ForwardSlaPolicyNameLicenseAssignmentApprovalSink : IForwardSlaPolicyNameTarget
{
    private readonly ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient _client;

    public ForwardSlaPolicyNameLicenseAssignmentApprovalSink(ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient client) => _client = client;

    public async Task PushAsync(ForwardSlaPolicyNameRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiLicensing.SubmitLicenseAssignmentRequest { LicenseAssignment = new ApiLicensing.LicenseAssignment { Number = record.ReferenceId }, Reason = record.SlaPolicyId }, token, _client.SubmitLicenseAssignmentAsync);
    }
}

/// <summary>Sends records to acme.licensing.v1.LicenseAssignmentLifecycleService.</summary>
internal sealed class ForwardSlaPolicyNameLicenseAssignmentLifecycleTarget : IForwardSlaPolicyNameTarget
{
    private readonly ApiLicensing.LicenseAssignmentLifecycleService.LicenseAssignmentLifecycleServiceClient _client;

    public ForwardSlaPolicyNameLicenseAssignmentLifecycleTarget(ApiLicensing.LicenseAssignmentLifecycleService.LicenseAssignmentLifecycleServiceClient client) => _client = client;

    public async Task PushAsync(ForwardSlaPolicyNameRecord record, CancellationToken token)
    {
        await _client.SuspendLicenseAssignmentAsync(new ApiLicensing.SuspendLicenseAssignmentRequest { LicenseAssignmentId = record.SlaPolicyId, Details = new ApiLicensing.SuspendLicenseAssignmentRequest.Types.Details { ReasonCode = record.ReferenceId }, Reason = record.Name }, cancellationToken: token);
    }
}

/// <summary>Reads acme.helpdesk.v1.SlaPolicyValidationService/ValidateSlaPolicy and hands what it keeps to the sink of its kind.</summary>
internal static class ForwardSlaPolicyName
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiHelpdesk.SlaPolicyValidationService.SlaPolicyValidationServiceClient(channel);
        var reply = await source.ValidateSlaPolicyAsync(new ApiHelpdesk.ValidateSlaPolicyRequest() { Reason = "current-forward-sla-policy-name" }, cancellationToken: token);
        var record = new ForwardSlaPolicyNameRecord
        {
            Name = reply.SlaPolicy.Name,
            ReferenceId = reply.Outcome.ReferenceId,
            SlaPolicyId = reply.SlaPolicy.SlaPolicyId,
        };
        await ActOnForwardSlaPolicyNameAsync("license-assignment-approval", record, channel, token);
    }

    private static async Task ActOnForwardSlaPolicyNameAsync(string kind, ForwardSlaPolicyNameRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["license-assignment-approval"] = () => new ForwardSlaPolicyNameLicenseAssignmentApprovalSink(new ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient(channel)).PushAsync(record, token),
            ["license-assignment-lifecycle"] = () => new ForwardSlaPolicyNameLicenseAssignmentLifecycleTarget(new ApiLicensing.LicenseAssignmentLifecycleService.LicenseAssignmentLifecycleServiceClient(channel)).PushAsync(record, token),
        };
        await actions[kind]();
    }
}
