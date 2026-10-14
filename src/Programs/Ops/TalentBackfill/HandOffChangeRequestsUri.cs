using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Ops.TalentBackfill;

/// <summary>Where HandOffChangeRequestsUri hands each record.</summary>
internal interface IHandOffChangeRequestsUriWriter
{
    Task DeliverAsync(HandOffChangeRequestsUriRecord record, CancellationToken token);
}

/// <summary>What HandOffChangeRequestsUri keeps of each SummarizeChangeRequests response.</summary>
internal sealed class HandOffChangeRequestsUriRecord
{
    public string DownloadUri { get; init; } = "";
    public string Message { get; init; } = "";
    public string Timezone { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Writes records to acme.timekeeping.v1.TimesheetQueryService.</summary>
internal sealed class HandOffChangeRequestsUriTimesheetQueryTarget : IHandOffChangeRequestsUriWriter
{
    private readonly ApiTimekeeping.TimesheetQueryService.TimesheetQueryServiceClient _client;

    public HandOffChangeRequestsUriTimesheetQueryTarget(ApiTimekeeping.TimesheetQueryService.TimesheetQueryServiceClient client) => _client = client;

    public async Task DeliverAsync(HandOffChangeRequestsUriRecord record, CancellationToken token)
    {
        await _client.CountTimesheetsAsync(new ApiTimekeeping.CountTimesheetsRequest { Filter = new ApiTimekeeping.CountTimesheetsRequest.Types.Filter { Query = record.DownloadUri, OwnerId = record.Message }, OrderBy = record.Timezone }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.corporateevents.v1.VenueCalculationService.</summary>
internal sealed class HandOffChangeRequestsUriVenueCalculationTarget : IHandOffChangeRequestsUriWriter
{
    private readonly ApiCorporateevents.VenueCalculationService.VenueCalculationServiceClient _client;

    public HandOffChangeRequestsUriVenueCalculationTarget(ApiCorporateevents.VenueCalculationService.VenueCalculationServiceClient client) => _client = client;

    public async Task DeliverAsync(HandOffChangeRequestsUriRecord record, CancellationToken token)
    {
        await _client.RecalculateVenuesAsync(new ApiCorporateevents.RecalculateVenuesRequest { Details = new ApiCorporateevents.RecalculateVenuesRequest.Types.Details { CurrencyCode = record.DownloadUri }, Venue = new ApiCorporateevents.Venue { Name = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.itsm.v1.IncidentService.</summary>
internal sealed class HandOffChangeRequestsUriIncidentWriter : IHandOffChangeRequestsUriWriter
{
    private readonly ApiItsm.IncidentService.IncidentServiceClient _client;

    public HandOffChangeRequestsUriIncidentWriter(ApiItsm.IncidentService.IncidentServiceClient client) => _client = client;

    public async Task DeliverAsync(HandOffChangeRequestsUriRecord record, CancellationToken token)
    {
        await _client.DeleteIncidentAsync(new ApiItsm.DeleteIncidentRequest { Etag = record.Timezone, Reason = record.Message, IncidentId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class HandOffChangeRequestsUriLogOnly : IHandOffChangeRequestsUriWriter
{
    public Task DeliverAsync(HandOffChangeRequestsUriRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.DownloadUri}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.itsm.v1.ChangeRequestQueryService/SummarizeChangeRequests and hands what it keeps to the sink of its kind.</summary>
internal static class HandOffChangeRequestsUri
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiItsm.ChangeRequestQueryService.ChangeRequestQueryServiceClient(channel);
        var response = await source.SummarizeChangeRequestsAsync(new ApiItsm.SummarizeChangeRequestsRequest() { ChangeRequestId = "nightly-hand-off-change-requests-uri" }, cancellationToken: token);
        var record = new HandOffChangeRequestsUriRecord
        {
            DownloadUri = response.DownloadUri,
            Message = Text.Normalize(response.Result.Message),
            Timezone = response.Period.Timezone,
            Status = Text.Normalize(response.Result.Status),
        };
        await DispatchHandOffChangeRequestsUriAsync("timesheet-query", record, channel, token);
    }

    private static async Task DispatchHandOffChangeRequestsUriAsync(string kind, HandOffChangeRequestsUriRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<HandOffChangeRequestsUriRecord, CancellationToken, Task> write = new HandOffChangeRequestsUriLogOnly().DeliverAsync;
        switch (kind)
        {
            case "timesheet-query":
                write = new HandOffChangeRequestsUriTimesheetQueryTarget(new ApiTimekeeping.TimesheetQueryService.TimesheetQueryServiceClient(channel)).DeliverAsync;
                break;
            case "venue-calculation":
                write = new HandOffChangeRequestsUriVenueCalculationTarget(new ApiCorporateevents.VenueCalculationService.VenueCalculationServiceClient(channel)).DeliverAsync;
                break;
            case "incident":
                write = new HandOffChangeRequestsUriIncidentWriter(new ApiItsm.IncidentService.IncidentServiceClient(channel)).DeliverAsync;
                break;
        }
        await write(record, token);
    }
}
