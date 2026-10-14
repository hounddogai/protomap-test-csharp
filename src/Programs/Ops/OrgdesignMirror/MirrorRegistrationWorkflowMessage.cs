using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.OrgdesignMirror;

/// <summary>Where MirrorRegistrationWorkflowMessage hands each record.</summary>
internal interface IMirrorRegistrationWorkflowMessageSink
{
    Task SendAsync(MirrorRegistrationWorkflowMessageRecord record, CancellationToken token);
}

/// <summary>What MirrorRegistrationWorkflowMessage keeps of each StartRegistrationWorkflow response.</summary>
internal sealed class MirrorRegistrationWorkflowMessageRecord
{
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Passes records to acme.documents.v1.SignatureValidationService.</summary>
internal sealed class MirrorRegistrationWorkflowMessageSignatureValidationRelay : IMirrorRegistrationWorkflowMessageSink
{
    private readonly ApiDocuments.SignatureValidationService.SignatureValidationServiceClient _client;

    public MirrorRegistrationWorkflowMessageSignatureValidationRelay(ApiDocuments.SignatureValidationService.SignatureValidationServiceClient client) => _client = client;

    public async Task SendAsync(MirrorRegistrationWorkflowMessageRecord record, CancellationToken token)
    {
        await _client.NormalizeSignatureAsync(new ApiDocuments.NormalizeSignatureRequest { Signature = new ApiDocuments.Signature { SignatureId = record.Message, CorrelationId = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.licensing.v1.LicenseAssignmentLifecycleService.</summary>
internal sealed class MirrorRegistrationWorkflowMessageLicenseAssignmentLifecycleTarget(ApiLicensing.LicenseAssignmentLifecycleService.LicenseAssignmentLifecycleServiceClient client) : IMirrorRegistrationWorkflowMessageSink
{
    public async Task SendAsync(MirrorRegistrationWorkflowMessageRecord record, CancellationToken token)
    {
        await client.ReinstateLicenseAssignmentAsync(new ApiLicensing.ReinstateLicenseAssignmentRequest { LicenseAssignmentId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.travel.v1.TravelBookingApprovalService.</summary>
internal sealed class MirrorRegistrationWorkflowMessageTravelBookingApprovalRelay : IMirrorRegistrationWorkflowMessageSink
{
    private readonly ApiTravel.TravelBookingApprovalService.TravelBookingApprovalServiceClient _client;

    public MirrorRegistrationWorkflowMessageTravelBookingApprovalRelay(ApiTravel.TravelBookingApprovalService.TravelBookingApprovalServiceClient client) => _client = client;

    public async Task SendAsync(MirrorRegistrationWorkflowMessageRecord record, CancellationToken token)
    {
        await _client.CommentTravelBookingApprovalAsync(new ApiTravel.CommentTravelBookingApprovalRequest { Details = new ApiTravel.CommentTravelBookingApprovalRequest.Types.Details { ApproverEmail = record.Message }, TravelBooking = new ApiTravel.TravelBooking { Number = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.scheduling.v1.RosterWorkflowService.</summary>
internal sealed class MirrorRegistrationWorkflowMessageRosterWorkflowSink(ApiScheduling.RosterWorkflowService.RosterWorkflowServiceClient client) : IMirrorRegistrationWorkflowMessageSink
{
    public async Task SendAsync(MirrorRegistrationWorkflowMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiScheduling.SignalRosterWorkflowRequest { RosterId = record.Status }, token, client.SignalRosterWorkflowAsync);
    }
}

/// <summary>Reads acme.corporateevents.v1.RegistrationWorkflowService/StartRegistrationWorkflow and hands what it keeps to the sink of its kind.</summary>
internal static class MirrorRegistrationWorkflowMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiCorporateevents.RegistrationWorkflowService.RegistrationWorkflowServiceClient(channel);
        var got = await source.StartRegistrationWorkflowAsync(new ApiCorporateevents.StartRegistrationWorkflowRequest() { Reason = "live-mirror-registration-workflow-message" }, cancellationToken: token);
        var record = new MirrorRegistrationWorkflowMessageRecord
        {
            Message = got.Outcome.Message,
            Status = got.Outcome.Status,
        };
        await ActOnMirrorRegistrationWorkflowMessageAsync("signature-validation", record, channel, token);
    }

    private static async Task ActOnMirrorRegistrationWorkflowMessageAsync(string kind, MirrorRegistrationWorkflowMessageRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["signature-validation"] = () => new MirrorRegistrationWorkflowMessageSignatureValidationRelay(new ApiDocuments.SignatureValidationService.SignatureValidationServiceClient(channel)).SendAsync(record, token),
            ["license-assignment-lifecycle"] = () => new MirrorRegistrationWorkflowMessageLicenseAssignmentLifecycleTarget(new ApiLicensing.LicenseAssignmentLifecycleService.LicenseAssignmentLifecycleServiceClient(channel)).SendAsync(record, token),
            ["travel-booking-approval"] = () => new MirrorRegistrationWorkflowMessageTravelBookingApprovalRelay(new ApiTravel.TravelBookingApprovalService.TravelBookingApprovalServiceClient(channel)).SendAsync(record, token),
            ["roster-workflow"] = () => new MirrorRegistrationWorkflowMessageRosterWorkflowSink(new ApiScheduling.RosterWorkflowService.RosterWorkflowServiceClient(channel)).SendAsync(record, token),
        };
        await actions[kind]();
    }
}
