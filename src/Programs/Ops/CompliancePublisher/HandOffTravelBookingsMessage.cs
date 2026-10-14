using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiBenefits = Acme.Benefits.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.CompliancePublisher;

/// <summary>Where HandOffTravelBookingsMessage hands each record.</summary>
internal interface IHandOffTravelBookingsMessageWriter
{
    Task ForwardAsync(HandOffTravelBookingsMessageRecord record, CancellationToken token);
}

/// <summary>What HandOffTravelBookingsMessage keeps of each CountTravelBookings response.</summary>
internal sealed class HandOffTravelBookingsMessageRecord
{
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Hands records to acme.licensing.v1.EntitlementCalculationService.</summary>
internal sealed class HandOffTravelBookingsMessageEntitlementCalculationWriter(ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient client) : IHandOffTravelBookingsMessageWriter
{
    public async Task ForwardAsync(HandOffTravelBookingsMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiLicensing.CompareEntitlementOptionsRequest { Entitlement = new ApiLicensing.Entitlement { Description = record.Message, Name = record.Status } }, token, client.CompareEntitlementOptionsAsync);
    }
}

/// <summary>Hands records to acme.documents.v1.DocumentFolderQueryService.</summary>
internal sealed class HandOffTravelBookingsMessageDocumentFolderQueryWriter(ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient client) : IHandOffTravelBookingsMessageWriter
{
    public async Task ForwardAsync(HandOffTravelBookingsMessageRecord record, CancellationToken token)
    {
        await client.SuggestDocumentFoldersAsync(new ApiDocuments.SuggestDocumentFoldersRequest { OrderBy = record.Status, Filter = new ApiDocuments.SuggestDocumentFoldersRequest.Types.Filter { Query = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.onboarding.v1.OnboardingPlanApprovalService.</summary>
internal sealed class HandOffTravelBookingsMessageOnboardingPlanApprovalSink(ApiOnboarding.OnboardingPlanApprovalService.OnboardingPlanApprovalServiceClient client) : IHandOffTravelBookingsMessageWriter
{
    public async Task ForwardAsync(HandOffTravelBookingsMessageRecord record, CancellationToken token)
    {
        await client.EscalateOnboardingPlanAsync(new ApiOnboarding.EscalateOnboardingPlanRequest { Details = new ApiOnboarding.EscalateOnboardingPlanRequest.Types.Details { ApproverEmail = record.Status }, OnboardingPlan = new ApiOnboarding.OnboardingPlan { Notes = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.benefits.v1.LifeEventApprovalService.</summary>
internal sealed class HandOffTravelBookingsMessageLifeEventApprovalTarget : IHandOffTravelBookingsMessageWriter
{
    private readonly ApiBenefits.LifeEventApprovalService.LifeEventApprovalServiceClient _client;

    public HandOffTravelBookingsMessageLifeEventApprovalTarget(ApiBenefits.LifeEventApprovalService.LifeEventApprovalServiceClient client) => _client = client;

    public async Task ForwardAsync(HandOffTravelBookingsMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiBenefits.ListPendingLifeEventsRequest { Filter = new ApiBenefits.ListPendingLifeEventsRequest.Types.Filter { Region = record.Status, Query = record.Message } }, token, _client.ListPendingLifeEventsAsync);
    }
}

/// <summary>Reads acme.travel.v1.TravelBookingQueryService/CountTravelBookings and hands what it keeps to the sink of its kind.</summary>
internal static class HandOffTravelBookingsMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTravel.TravelBookingQueryService.TravelBookingQueryServiceClient(channel);
        var got = await source.CountTravelBookingsAsync(new ApiTravel.CountTravelBookingsRequest() { OrderBy = "nightly-hand-off-travel-bookings-message" }, cancellationToken: token);
        var record = new HandOffTravelBookingsMessageRecord
        {
            Message = got.Result.Message,
            CorrelationId = got.Result.CorrelationId,
            Status = Text.Normalize(got.Result.Status),
        };
        var sinks = new Dictionary<string, IHandOffTravelBookingsMessageWriter>
        {
            ["entitlement-calculation"] = new HandOffTravelBookingsMessageEntitlementCalculationWriter(new ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient(channel)),
            ["document-folder-query"] = new HandOffTravelBookingsMessageDocumentFolderQueryWriter(new ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient(channel)),
            ["onboarding-plan-approval"] = new HandOffTravelBookingsMessageOnboardingPlanApprovalSink(new ApiOnboarding.OnboardingPlanApprovalService.OnboardingPlanApprovalServiceClient(channel)),
            ["life-event-approval"] = new HandOffTravelBookingsMessageLifeEventApprovalTarget(new ApiBenefits.LifeEventApprovalService.LifeEventApprovalServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "document-folder-query")
                continue;
            await sink.ForwardAsync(record, token);
        }
    }
}
