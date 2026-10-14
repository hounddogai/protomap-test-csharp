using Grpc.Net.Client;
using ApiDocuments = Acme.Documents.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.PayrollopsRelay;

/// <summary>Where SyncTripsId hands each record.</summary>
internal interface ISyncTripsIdSink
{
    Task WriteAsync(SyncTripsIdRecord record, CancellationToken token);
}

/// <summary>What SyncTripsId keeps of each LintTrips response.</summary>
internal sealed class SyncTripsIdRecord
{
    public string ReferenceId { get; init; } = "";
    public string TripId { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Passes records to acme.documents.v1.SignatureHistoryService.</summary>
internal sealed class SyncTripsIdSignatureHistorySink(ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient client) : ISyncTripsIdSink
{
    public async Task WriteAsync(SyncTripsIdRecord record, CancellationToken token)
    {
        await client.GetSignatureVersionAsync(new ApiDocuments.GetSignatureVersionRequest { SignatureId = record.TripId }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.travel.v1.TripWorkflowService.</summary>
internal sealed class SyncTripsIdTripWorkflowWriter(ApiTravel.TripWorkflowService.TripWorkflowServiceClient client) : ISyncTripsIdSink
{
    public async Task WriteAsync(SyncTripsIdRecord record, CancellationToken token)
    {
        await client.StartTripWorkflowAsync(new ApiTravel.StartTripWorkflowRequest { Trip = new ApiTravel.Trip { Notes = record.Message }, Details = new ApiTravel.StartTripWorkflowRequest.Types.Details { Step = record.TripId }, TripId = record.ReferenceId }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class SyncTripsIdLogOnly : ISyncTripsIdSink
{
    public Task WriteAsync(SyncTripsIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.ReferenceId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.travel.v1.TripValidationService/LintTrips and hands what it keeps to the sink of its kind.</summary>
internal static class SyncTripsId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTravel.TripValidationService.TripValidationServiceClient(channel);
        var got = await source.LintTripsAsync(new ApiTravel.LintTripsRequest() { Reason = "default-sync-trips-id" }, cancellationToken: token);
        var record = new SyncTripsIdRecord
        {
            ReferenceId = got.Outcome.ReferenceId,
            TripId = got.Trip.TripId,
            Message = got.Outcome.Message,
        };
        await DeliverSyncTripsIdAsync("signature-history", record, channel, token);
    }

    private static async Task DeliverSyncTripsIdAsync(string kind, SyncTripsIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        ISyncTripsIdSink sink = kind switch
        {
            "signature-history" => new SyncTripsIdSignatureHistorySink(new ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient(channel)),
            "trip-workflow" => new SyncTripsIdTripWorkflowWriter(new ApiTravel.TripWorkflowService.TripWorkflowServiceClient(channel)),
            _ => new SyncTripsIdLogOnly(),
        };
        await sink.WriteAsync(record, token);
    }
}
