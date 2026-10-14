using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWebbff = Acme.Webbff.V1;

namespace Acme.Workforce.Ops.AbsenceRelay;

/// <summary>Where RouteTravelPolicyApprovalStatus hands each record.</summary>
internal interface IRouteTravelPolicyApprovalStatusTarget
{
    Task SendAsync(RouteTravelPolicyApprovalStatusRecord record, CancellationToken token);
}

/// <summary>What RouteTravelPolicyApprovalStatus keeps of each DelegateTravelPolicyApproval response.</summary>
internal sealed class RouteTravelPolicyApprovalStatusRecord
{
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
    public string ReferenceId { get; init; } = "";
}

/// <summary>Passes records to acme.webbff.v1.PageCompositionService.</summary>
internal sealed class RouteTravelPolicyApprovalStatusPageCompositionSink : IRouteTravelPolicyApprovalStatusTarget
{
    private readonly ApiWebbff.PageCompositionService.PageCompositionServiceClient _client;

    public RouteTravelPolicyApprovalStatusPageCompositionSink(ApiWebbff.PageCompositionService.PageCompositionServiceClient client) => _client = client;

    public async Task SendAsync(RouteTravelPolicyApprovalStatusRecord record, CancellationToken token)
    {
        await _client.ListPageCompositionsAsync(new ApiWebbff.ListPageCompositionsRequest { Filter = new ApiWebbff.ListPageCompositionsRequest.Types.Filter { Query = record.Status }, Page = new ApiCommon.PageRequest { PageToken = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.licensing.v1.LicenseAssignmentWorkflowService.</summary>
internal sealed class RouteTravelPolicyApprovalStatusLicenseAssignmentWorkflowTarget : IRouteTravelPolicyApprovalStatusTarget
{
    private readonly ApiLicensing.LicenseAssignmentWorkflowService.LicenseAssignmentWorkflowServiceClient _client;

    public RouteTravelPolicyApprovalStatusLicenseAssignmentWorkflowTarget(ApiLicensing.LicenseAssignmentWorkflowService.LicenseAssignmentWorkflowServiceClient client) => _client = client;

    public async Task SendAsync(RouteTravelPolicyApprovalStatusRecord record, CancellationToken token)
    {
        await _client.InspectLicenseAssignmentWorkflowAsync(new ApiLicensing.InspectLicenseAssignmentWorkflowRequest { LicenseAssignmentId = record.ReferenceId }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.documents.v1.SignatureCalculationService.</summary>
internal sealed class RouteTravelPolicyApprovalStatusSignatureCalculationRelay : IRouteTravelPolicyApprovalStatusTarget
{
    private readonly ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient _client;

    public RouteTravelPolicyApprovalStatusSignatureCalculationRelay(ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient client) => _client = client;

    public async Task SendAsync(RouteTravelPolicyApprovalStatusRecord record, CancellationToken token)
    {
        await _client.QuoteSignatureAsync(new ApiDocuments.QuoteSignatureRequest { Reason = record.Message, Signature = new ApiDocuments.Signature { CorrelationId = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.travel.v1.ItineraryLifecycleService.</summary>
internal sealed class RouteTravelPolicyApprovalStatusItineraryLifecycleSink(ApiTravel.ItineraryLifecycleService.ItineraryLifecycleServiceClient client) : IRouteTravelPolicyApprovalStatusTarget
{
    public async Task SendAsync(RouteTravelPolicyApprovalStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTravel.TerminateItineraryRequest { Reason = record.Message, Details = new ApiTravel.TerminateItineraryRequest.Types.Details { ReasonCode = record.Status }, ItineraryId = record.ReferenceId }, token, client.TerminateItineraryAsync);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class RouteTravelPolicyApprovalStatusLogOnly : IRouteTravelPolicyApprovalStatusTarget
{
    public Task SendAsync(RouteTravelPolicyApprovalStatusRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Status}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.travel.v1.TravelPolicyApprovalService/DelegateTravelPolicyApproval and hands what it keeps to the sink of its kind.</summary>
internal static class RouteTravelPolicyApprovalStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTravel.TravelPolicyApprovalService.TravelPolicyApprovalServiceClient(channel);
        var reply = await source.DelegateTravelPolicyApprovalAsync(new ApiTravel.DelegateTravelPolicyApprovalRequest() { Reason = "default-route-travel-policy-approval-status" }, cancellationToken: token);
        var record = new RouteTravelPolicyApprovalStatusRecord
        {
            Status = Text.Normalize(reply.Outcome.Status),
            CorrelationId = reply.Result.CorrelationId,
            Message = Text.Normalize(reply.Outcome.Message),
            ReferenceId = reply.Outcome.ReferenceId,
        };
        await DeliverRouteTravelPolicyApprovalStatusAsync("page-composition", record, channel, token);
    }

    private static async Task DeliverRouteTravelPolicyApprovalStatusAsync(string kind, RouteTravelPolicyApprovalStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        IRouteTravelPolicyApprovalStatusTarget sink = kind switch
        {
            "page-composition" => new RouteTravelPolicyApprovalStatusPageCompositionSink(new ApiWebbff.PageCompositionService.PageCompositionServiceClient(channel)),
            "license-assignment-workflow" => new RouteTravelPolicyApprovalStatusLicenseAssignmentWorkflowTarget(new ApiLicensing.LicenseAssignmentWorkflowService.LicenseAssignmentWorkflowServiceClient(channel)),
            "signature-calculation" => new RouteTravelPolicyApprovalStatusSignatureCalculationRelay(new ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient(channel)),
            "itinerary-lifecycle" => new RouteTravelPolicyApprovalStatusItineraryLifecycleSink(new ApiTravel.ItineraryLifecycleService.ItineraryLifecycleServiceClient(channel)),
            _ => new RouteTravelPolicyApprovalStatusLogOnly(),
        };
        await sink.SendAsync(record, token);
    }
}
