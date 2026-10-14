using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiPrescriptions = Acme.Prescriptions.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.OrgdesignMirror;

/// <summary>Where RouteDocumentTestNoticeNotes hands each record.</summary>
internal interface IRouteDocumentTestNoticeNotesSink
{
    Task PushAsync(RouteDocumentTestNoticeNotesRecord record, CancellationToken token);
}

/// <summary>What RouteDocumentTestNoticeNotes keeps of each SendDocumentTestNotice response.</summary>
internal sealed class RouteDocumentTestNoticeNotesRecord
{
    public string Notes { get; init; } = "";
    public string Status { get; init; } = "";
    public string OutcomeStatus { get; init; } = "";
    public string Number { get; init; } = "";
}

/// <summary>Hands records to acme.onboarding.v1.OnboardingTaskWorkflowService.</summary>
internal sealed class RouteDocumentTestNoticeNotesOnboardingTaskWorkflowWriter(ApiOnboarding.OnboardingTaskWorkflowService.OnboardingTaskWorkflowServiceClient client) : IRouteDocumentTestNoticeNotesSink
{
    public async Task PushAsync(RouteDocumentTestNoticeNotesRecord record, CancellationToken token)
    {
        await client.ListOnboardingTaskWorkflowsAsync(new ApiOnboarding.ListOnboardingTaskWorkflowsRequest { Page = new ApiCommon.PageRequest { PageToken = record.OutcomeStatus }, Filter = new ApiOnboarding.ListOnboardingTaskWorkflowsRequest.Types.Filter { Query = record.Notes }, OrderBy = record.Number }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.travel.v1.TripService.</summary>
internal sealed class RouteDocumentTestNoticeNotesTripTarget(ApiTravel.TripService.TripServiceClient client) : IRouteDocumentTestNoticeNotesSink
{
    public async Task PushAsync(RouteDocumentTestNoticeNotesRecord record, CancellationToken token)
    {
        await client.GetTripAsync(new ApiTravel.GetTripRequest { TripId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.prescriptions.v1.PrescriptionQueryService.</summary>
internal sealed class RouteDocumentTestNoticeNotesPrescriptionQuerySink(ApiPrescriptions.PrescriptionQueryService.PrescriptionQueryServiceClient client) : IRouteDocumentTestNoticeNotesSink
{
    public async Task PushAsync(RouteDocumentTestNoticeNotesRecord record, CancellationToken token)
    {
        await client.FindPrescriptionsAsync(new ApiPrescriptions.FindPrescriptionsRequest { Filter = new ApiPrescriptions.FindPrescriptionsRequest.Types.Filter { Region = record.Notes } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.documents.v1.DocumentNotificationService/SendDocumentTestNotice and hands what it keeps to the sink of its kind.</summary>
internal static class RouteDocumentTestNoticeNotes
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient(channel);
        var reply = await source.SendDocumentTestNoticeAsync(new ApiDocuments.SendDocumentTestNoticeRequest() { DocumentId = "main-route-document-test-notice-notes" }, cancellationToken: token);
        var record = new RouteDocumentTestNoticeNotesRecord
        {
            Notes = Text.Normalize(reply.Document.Notes),
            Status = Text.Normalize(reply.Result.Status),
            OutcomeStatus = reply.Outcome.Status,
            Number = reply.Document.Number,
        };
        await ActOnRouteDocumentTestNoticeNotesAsync("prescription-query", record, channel, token);
    }

    private static async Task ActOnRouteDocumentTestNoticeNotesAsync(string kind, RouteDocumentTestNoticeNotesRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["onboarding-task-workflow"] = () => new RouteDocumentTestNoticeNotesOnboardingTaskWorkflowWriter(new ApiOnboarding.OnboardingTaskWorkflowService.OnboardingTaskWorkflowServiceClient(channel)).PushAsync(record, token),
            ["trip"] = () => new RouteDocumentTestNoticeNotesTripTarget(new ApiTravel.TripService.TripServiceClient(channel)).PushAsync(record, token),
            ["prescription-query"] = () => new RouteDocumentTestNoticeNotesPrescriptionQuerySink(new ApiPrescriptions.PrescriptionQueryService.PrescriptionQueryServiceClient(channel)).PushAsync(record, token),
        };
        await actions[kind]();
    }
}
