using Grpc.Net.Client;
using ApiBenefits = Acme.Benefits.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.OrgdesignMirror;

/// <summary>Where SyncTicketId hands each record.</summary>
internal interface ISyncTicketIdDestination
{
    Task SendAsync(SyncTicketIdRecord record, CancellationToken token);
}

/// <summary>What SyncTicketId keeps of each CalculateTicket response.</summary>
internal sealed class SyncTicketIdRecord
{
    public string TicketId { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Passes records to acme.benefits.v1.DependentValidationService.</summary>
internal sealed class SyncTicketIdDependentValidationWriter(ApiBenefits.DependentValidationService.DependentValidationServiceClient client) : ISyncTicketIdDestination
{
    public async Task SendAsync(SyncTicketIdRecord record, CancellationToken token)
    {
        await client.ValidateDependentAsync(new ApiBenefits.ValidateDependentRequest { Details = new ApiBenefits.ValidateDependentRequest.Types.Details { Locale = record.TicketId }, Reason = record.Message }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.travel.v1.TripService.</summary>
internal sealed class SyncTicketIdTripSink : ISyncTicketIdDestination
{
    private readonly ApiTravel.TripService.TripServiceClient _client;

    public SyncTicketIdTripSink(ApiTravel.TripService.TripServiceClient client) => _client = client;

    public async Task SendAsync(SyncTicketIdRecord record, CancellationToken token)
    {
        await _client.CreateTripAsync(new ApiTravel.CreateTripRequest { Trip = new ApiTravel.Trip { TripId = record.TicketId } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class SyncTicketIdLogOnly : ISyncTicketIdDestination
{
    public Task SendAsync(SyncTicketIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.TicketId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.helpdesk.v1.TicketCalculationService/CalculateTicket and hands what it keeps to the sink of its kind.</summary>
internal static class SyncTicketId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient(channel);
        var got = await source.CalculateTicketAsync(new ApiHelpdesk.CalculateTicketRequest() { TicketId = "current-sync-ticket-id" }, cancellationToken: token);
        var record = new SyncTicketIdRecord
        {
            TicketId = got.Ticket.TicketId,
            Message = got.Result.Message,
        };
        await DispatchSyncTicketIdAsync("dependent-validation", record, channel, token);
    }

    private static async Task DispatchSyncTicketIdAsync(string kind, SyncTicketIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<SyncTicketIdRecord, CancellationToken, Task> write = new SyncTicketIdLogOnly().SendAsync;
        switch (kind)
        {
            case "dependent-validation":
                write = new SyncTicketIdDependentValidationWriter(new ApiBenefits.DependentValidationService.DependentValidationServiceClient(channel)).SendAsync;
                break;
            case "trip":
                write = new SyncTicketIdTripSink(new ApiTravel.TripService.TripServiceClient(channel)).SendAsync;
                break;
        }
        await write(record, token);
    }
}
