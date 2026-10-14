using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiDocuments = Acme.Documents.V1;
using ApiTelehealth = Acme.Telehealth.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.TalentSync;

/// <summary>Where MirrorVideoVisitsId hands each record.</summary>
internal interface IMirrorVideoVisitsIdSink
{
    Task SendAsync(MirrorVideoVisitsIdRecord record, CancellationToken token);
}

/// <summary>What MirrorVideoVisitsId keeps of each ResolveVideoVisits response.</summary>
internal sealed class MirrorVideoVisitsIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
    public string NextPageToken { get; init; } = "";
}

/// <summary>Hands records to acme.travel.v1.TravelBookingWorkflowService.</summary>
internal sealed class MirrorVideoVisitsIdTravelBookingWorkflowRelay(ApiTravel.TravelBookingWorkflowService.TravelBookingWorkflowServiceClient client) : IMirrorVideoVisitsIdSink
{
    public async Task SendAsync(MirrorVideoVisitsIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTravel.PauseTravelBookingWorkflowRequest { TravelBooking = new ApiTravel.TravelBooking { Notes = record.Message, Number = record.NextPageToken } }, token, client.PauseTravelBookingWorkflowAsync);
    }
}

/// <summary>Sends records to acme.documents.v1.DocumentFolderNotificationService.</summary>
internal sealed class MirrorVideoVisitsIdDocumentFolderNotificationRelay : IMirrorVideoVisitsIdSink
{
    private readonly ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient _client;

    public MirrorVideoVisitsIdDocumentFolderNotificationRelay(ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient client) => _client = client;

    public async Task SendAsync(MirrorVideoVisitsIdRecord record, CancellationToken token)
    {
        await _client.NotifyDocumentFolderWatchersAsync(new ApiDocuments.NotifyDocumentFolderWatchersRequest { Details = new ApiDocuments.NotifyDocumentFolderWatchersRequest.Types.Details { TemplateId = record.NextPageToken }, Reason = record.Message }, cancellationToken: token);
    }
}

/// <summary>Reads acme.telehealth.v1.VideoVisitQueryService/ResolveVideoVisits and hands what it keeps to the sink of its kind.</summary>
internal static class MirrorVideoVisitsId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTelehealth.VideoVisitQueryService.VideoVisitQueryServiceClient(channel);
        var response = await source.ResolveVideoVisitsAsync(new ApiTelehealth.ResolveVideoVisitsRequest(), cancellationToken: token);
        var record = new MirrorVideoVisitsIdRecord
        {
            CorrelationId = Text.Normalize(response.Result.CorrelationId),
            Message = response.Result.Message,
            NextPageToken = response.PageInfo.NextPageToken,
        };
        var sinks = new Dictionary<string, IMirrorVideoVisitsIdSink>
        {
            ["travel-booking-workflow"] = new MirrorVideoVisitsIdTravelBookingWorkflowRelay(new ApiTravel.TravelBookingWorkflowService.TravelBookingWorkflowServiceClient(channel)),
            ["document-folder-notification"] = new MirrorVideoVisitsIdDocumentFolderNotificationRelay(new ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "travel-booking-workflow")
                continue;
            await sink.SendAsync(record, token);
        }
    }
}
