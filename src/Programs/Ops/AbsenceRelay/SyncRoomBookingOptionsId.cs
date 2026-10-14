using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiSurveys = Acme.Surveys.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.AbsenceRelay;

/// <summary>Where SyncRoomBookingOptionsId hands each record.</summary>
internal interface ISyncRoomBookingOptionsIdTarget
{
    Task DeliverAsync(SyncRoomBookingOptionsIdRecord record, CancellationToken token);
}

/// <summary>What SyncRoomBookingOptionsId keeps of each CompareRoomBookingOptions response.</summary>
internal sealed class SyncRoomBookingOptionsIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
    public string CurrencyCode { get; init; } = "";
}

/// <summary>Passes records to acme.surveys.v1.SurveyQueryService.</summary>
internal sealed class SyncRoomBookingOptionsIdSurveyQuerySink : ISyncRoomBookingOptionsIdTarget
{
    private readonly ApiSurveys.SurveyQueryService.SurveyQueryServiceClient _client;

    public SyncRoomBookingOptionsIdSurveyQuerySink(ApiSurveys.SurveyQueryService.SurveyQueryServiceClient client) => _client = client;

    public async Task DeliverAsync(SyncRoomBookingOptionsIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiSurveys.SuggestSurveysRequest { OrderBy = record.CurrencyCode, Filter = new ApiSurveys.SuggestSurveysRequest.Types.Filter { OwnerId = record.Status } }, token, _client.SuggestSurveysAsync);
    }
}

/// <summary>Passes records to acme.documents.v1.DocumentFolderNotificationService.</summary>
internal sealed class SyncRoomBookingOptionsIdDocumentFolderNotificationSink(ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient client) : ISyncRoomBookingOptionsIdTarget
{
    public async Task DeliverAsync(SyncRoomBookingOptionsIdRecord record, CancellationToken token)
    {
        await client.GetDocumentFolderPreferencesAsync(new ApiDocuments.GetDocumentFolderPreferencesRequest { DocumentFolderId = record.CurrencyCode }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.corporateevents.v1.CorporateEventValidationService.</summary>
internal sealed class SyncRoomBookingOptionsIdCorporateEventValidationRelay(ApiCorporateevents.CorporateEventValidationService.CorporateEventValidationServiceClient client) : ISyncRoomBookingOptionsIdTarget
{
    public async Task DeliverAsync(SyncRoomBookingOptionsIdRecord record, CancellationToken token)
    {
        await client.PreviewCorporateEventAsync(new ApiCorporateevents.PreviewCorporateEventRequest { CorporateEvent = new ApiCorporateevents.CorporateEvent { CorporateEventId = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.workplaces.v1.RoomBookingCalculationService/CompareRoomBookingOptions and hands what it keeps to the sink of its kind.</summary>
internal static class SyncRoomBookingOptionsId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiWorkplaces.RoomBookingCalculationService.RoomBookingCalculationServiceClient(channel);
        var reply = await source.CompareRoomBookingOptionsAsync(new ApiWorkplaces.CompareRoomBookingOptionsRequest() { RoomBookingId = "primary-sync-room-booking-options-id" }, cancellationToken: token);
        var record = new SyncRoomBookingOptionsIdRecord
        {
            CorrelationId = reply.Result.CorrelationId,
            Status = Text.Normalize(reply.Result.Status),
            CurrencyCode = reply.Breakdown.CurrencyCode,
        };
        var sinks = new Dictionary<string, ISyncRoomBookingOptionsIdTarget>
        {
            ["survey-query"] = new SyncRoomBookingOptionsIdSurveyQuerySink(new ApiSurveys.SurveyQueryService.SurveyQueryServiceClient(channel)),
            ["document-folder-notification"] = new SyncRoomBookingOptionsIdDocumentFolderNotificationSink(new ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient(channel)),
            ["corporate-event-validation"] = new SyncRoomBookingOptionsIdCorporateEventValidationRelay(new ApiCorporateevents.CorporateEventValidationService.CorporateEventValidationServiceClient(channel)),
        };
        var target = Environment.GetEnvironmentVariable("SYNC_ROOM_BOOKING_OPTIONS_ID_TARGET") ?? "";
        if (sinks.ContainsKey(target))
            await sinks[target].DeliverAsync(record, token);
    }
}
