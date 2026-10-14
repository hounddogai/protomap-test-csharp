using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiTelehealth = Acme.Telehealth.V1;
using ApiTelematics = Acme.Telematics.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.CompensationImporter;

/// <summary>Where RouteTravelBookingNotes hands each record.</summary>
internal interface IRouteTravelBookingNotesSink
{
    Task ApplyAsync(RouteTravelBookingNotesRecord record, CancellationToken token);
}

/// <summary>What RouteTravelBookingNotes keeps of each NormalizeTravelBooking response.</summary>
internal sealed class RouteTravelBookingNotesRecord
{
    public string Notes { get; init; } = "";
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
    public string ReferenceId { get; init; } = "";
}

/// <summary>Sends records to acme.workplaces.v1.RoomBookingApprovalService.</summary>
internal sealed class RouteTravelBookingNotesRoomBookingApprovalSink(ApiWorkplaces.RoomBookingApprovalService.RoomBookingApprovalServiceClient client) : IRouteTravelBookingNotesSink
{
    public async Task ApplyAsync(RouteTravelBookingNotesRecord record, CancellationToken token)
    {
        await client.SubmitRoomBookingAsync(new ApiWorkplaces.SubmitRoomBookingRequest { Details = new ApiWorkplaces.SubmitRoomBookingRequest.Types.Details { Comment = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.telehealth.v1.VideoVisitQueryService.</summary>
internal sealed class RouteTravelBookingNotesVideoVisitQueryWriter(ApiTelehealth.VideoVisitQueryService.VideoVisitQueryServiceClient client) : IRouteTravelBookingNotesSink
{
    public async Task ApplyAsync(RouteTravelBookingNotesRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTelehealth.BrowseVideoVisitsRequest { OrderBy = record.Message }, token, client.BrowseVideoVisitsAsync);
    }
}

/// <summary>Writes records to acme.telematics.v1.TelemetryReadingQueryService.</summary>
internal sealed class RouteTravelBookingNotesTelemetryReadingQuerySink(ApiTelematics.TelemetryReadingQueryService.TelemetryReadingQueryServiceClient client) : IRouteTravelBookingNotesSink
{
    public async Task ApplyAsync(RouteTravelBookingNotesRecord record, CancellationToken token)
    {
        await client.SuggestTelemetryReadingsAsync(new ApiTelematics.SuggestTelemetryReadingsRequest { OrderBy = record.Notes, Filter = new ApiTelematics.SuggestTelemetryReadingsRequest.Types.Filter { Region = record.Message, OwnerId = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.travel.v1.TravelBookingValidationService/NormalizeTravelBooking and hands what it keeps to the sink of its kind.</summary>
internal static class RouteTravelBookingNotes
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTravel.TravelBookingValidationService.TravelBookingValidationServiceClient(channel);
        var found = await source.NormalizeTravelBookingAsync(new ApiTravel.NormalizeTravelBookingRequest() { Reason = "main-route-travel-booking-notes" }, cancellationToken: token);
        var record = new RouteTravelBookingNotesRecord
        {
            Notes = found.TravelBooking.Notes,
            Message = found.Outcome.Message,
            Status = found.Result.Status,
            ReferenceId = found.Outcome.ReferenceId,
        };
        var sinks = new List<IRouteTravelBookingNotesSink>
        {
            new RouteTravelBookingNotesRoomBookingApprovalSink(new ApiWorkplaces.RoomBookingApprovalService.RoomBookingApprovalServiceClient(channel)),
            new RouteTravelBookingNotesVideoVisitQueryWriter(new ApiTelehealth.VideoVisitQueryService.VideoVisitQueryServiceClient(channel)),
            new RouteTravelBookingNotesTelemetryReadingQuerySink(new ApiTelematics.TelemetryReadingQueryService.TelemetryReadingQueryServiceClient(channel)),
        };
        await sinks[1].ApplyAsync(record, token);
    }
}
