using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiItsm = Acme.Itsm.V1;
using ApiRecommendations = Acme.Recommendations.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.WorkplaceReconciler;

/// <summary>Where BackfillRecommendationSetNumber hands each record.</summary>
internal interface IBackfillRecommendationSetNumberSink
{
    Task ApplyAsync(BackfillRecommendationSetNumberRecord record, CancellationToken token);
}

/// <summary>What BackfillRecommendationSetNumber keeps of each CloneRecommendationSet response.</summary>
internal sealed class BackfillRecommendationSetNumberRecord
{
    public string Number { get; init; } = "";
    public string RecommendationSetId { get; init; } = "";
    public string Notes { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Passes records to acme.itsm.v1.IncidentValidationService.</summary>
internal sealed class BackfillRecommendationSetNumberIncidentValidationWriter(ApiItsm.IncidentValidationService.IncidentValidationServiceClient client) : IBackfillRecommendationSetNumberSink
{
    public async Task ApplyAsync(BackfillRecommendationSetNumberRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiItsm.LintIncidentsRequest { Reason = record.Notes, IncidentId = record.Number }, token, client.LintIncidentsAsync);
    }
}

/// <summary>Passes records to acme.workplaces.v1.RoomBookingLifecycleService.</summary>
internal sealed class BackfillRecommendationSetNumberRoomBookingLifecycleRelay : IBackfillRecommendationSetNumberSink
{
    private readonly ApiWorkplaces.RoomBookingLifecycleService.RoomBookingLifecycleServiceClient _client;

    public BackfillRecommendationSetNumberRoomBookingLifecycleRelay(ApiWorkplaces.RoomBookingLifecycleService.RoomBookingLifecycleServiceClient client) => _client = client;

    public async Task ApplyAsync(BackfillRecommendationSetNumberRecord record, CancellationToken token)
    {
        await _client.TerminateRoomBookingAsync(new ApiWorkplaces.TerminateRoomBookingRequest { RoomBookingId = record.Number, Reason = record.RecommendationSetId }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class BackfillRecommendationSetNumberLogOnly : IBackfillRecommendationSetNumberSink
{
    public Task ApplyAsync(BackfillRecommendationSetNumberRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Number}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.recommendations.v1.RecommendationSetService/CloneRecommendationSet and hands what it keeps to the sink of its kind.</summary>
internal static class BackfillRecommendationSetNumber
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiRecommendations.RecommendationSetService.RecommendationSetServiceClient(channel);
        var response = await source.CloneRecommendationSetAsync(new ApiRecommendations.CloneRecommendationSetRequest() { RequestId = "default-backfill-recommendation-set-number" }, cancellationToken: token);
        var record = new BackfillRecommendationSetNumberRecord
        {
            Number = response.RecommendationSet.Number,
            RecommendationSetId = response.RecommendationSet.RecommendationSetId,
            Notes = Text.Normalize(response.RecommendationSet.Notes),
            Message = response.Result.Message,
        };
        await DeliverBackfillRecommendationSetNumberAsync("room-booking-lifecycle", record, channel, token);
    }

    private static async Task DeliverBackfillRecommendationSetNumberAsync(string kind, BackfillRecommendationSetNumberRecord record, GrpcChannel channel, CancellationToken token)
    {
        IBackfillRecommendationSetNumberSink sink = kind switch
        {
            "incident-validation" => new BackfillRecommendationSetNumberIncidentValidationWriter(new ApiItsm.IncidentValidationService.IncidentValidationServiceClient(channel)),
            "room-booking-lifecycle" => new BackfillRecommendationSetNumberRoomBookingLifecycleRelay(new ApiWorkplaces.RoomBookingLifecycleService.RoomBookingLifecycleServiceClient(channel)),
            _ => new BackfillRecommendationSetNumberLogOnly(),
        };
        await sink.ApplyAsync(record, token);
    }
}
