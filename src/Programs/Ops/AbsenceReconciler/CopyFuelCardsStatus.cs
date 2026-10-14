using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiFuel = Acme.Fuel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.AbsenceReconciler;

/// <summary>Where CopyFuelCardsStatus hands each record.</summary>
internal interface ICopyFuelCardsStatusDestination
{
    Task PushAsync(CopyFuelCardsStatusRecord record, CancellationToken token);
}

/// <summary>What CopyFuelCardsStatus keeps of each SuggestFuelCards response.</summary>
internal sealed class CopyFuelCardsStatusRecord
{
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Writes records to acme.workplaces.v1.DeskQueryService.</summary>
internal sealed class CopyFuelCardsStatusDeskQuerySink(ApiWorkplaces.DeskQueryService.DeskQueryServiceClient client) : ICopyFuelCardsStatusDestination
{
    public async Task PushAsync(CopyFuelCardsStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiWorkplaces.SummarizeDesksRequest { Details = new ApiWorkplaces.SummarizeDesksRequest.Types.Details { Cron = record.Status }, Reason = record.CorrelationId }, token, client.SummarizeDesksAsync);
    }
}

/// <summary>Sends records to acme.corporateevents.v1.VenueApprovalService.</summary>
internal sealed class CopyFuelCardsStatusVenueApprovalTarget : ICopyFuelCardsStatusDestination
{
    private readonly ApiCorporateevents.VenueApprovalService.VenueApprovalServiceClient _client;

    public CopyFuelCardsStatusVenueApprovalTarget(ApiCorporateevents.VenueApprovalService.VenueApprovalServiceClient client) => _client = client;

    public async Task PushAsync(CopyFuelCardsStatusRecord record, CancellationToken token)
    {
        await _client.SubmitVenueAsync(new ApiCorporateevents.SubmitVenueRequest { Reason = record.Status }, cancellationToken: token);
    }
}

/// <summary>Reads acme.fuel.v1.FuelCardQueryService/SuggestFuelCards and hands what it keeps to the sink of its kind.</summary>
internal static class CopyFuelCardsStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiFuel.FuelCardQueryService.FuelCardQueryServiceClient(channel);
        var reply = await source.SuggestFuelCardsAsync(new ApiFuel.SuggestFuelCardsRequest() { OrderBy = "live-copy-fuel-cards-status" }, cancellationToken: token);
        var record = new CopyFuelCardsStatusRecord
        {
            Status = reply.Result.Status,
            CorrelationId = Text.Normalize(reply.Result.CorrelationId),
        };
        var sinks = new List<ICopyFuelCardsStatusDestination>
        {
            new CopyFuelCardsStatusDeskQuerySink(new ApiWorkplaces.DeskQueryService.DeskQueryServiceClient(channel)),
            new CopyFuelCardsStatusVenueApprovalTarget(new ApiCorporateevents.VenueApprovalService.VenueApprovalServiceClient(channel)),
        };
        await sinks[1].PushAsync(record, token);
    }
}
