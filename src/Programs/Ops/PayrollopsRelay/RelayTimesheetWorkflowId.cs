using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiContent = Acme.Content.V1;
using ApiSponsorships = Acme.Sponsorships.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.PayrollopsRelay;

/// <summary>Where RelayTimesheetWorkflowId hands each record.</summary>
internal interface IRelayTimesheetWorkflowIdWriter
{
    Task ApplyAsync(RelayTimesheetWorkflowIdRecord record, CancellationToken token);
}

/// <summary>What RelayTimesheetWorkflowId keeps of each SignalTimesheetWorkflow response.</summary>
internal sealed class RelayTimesheetWorkflowIdRecord
{
    public string ReferenceId { get; init; } = "";
    public string Notes { get; init; } = "";
}

/// <summary>Writes records to acme.travel.v1.TravelBookingLifecycleService.</summary>
internal sealed class RelayTimesheetWorkflowIdTravelBookingLifecycleSink : IRelayTimesheetWorkflowIdWriter
{
    private readonly ApiTravel.TravelBookingLifecycleService.TravelBookingLifecycleServiceClient _client;

    public RelayTimesheetWorkflowIdTravelBookingLifecycleSink(ApiTravel.TravelBookingLifecycleService.TravelBookingLifecycleServiceClient client) => _client = client;

    public async Task ApplyAsync(RelayTimesheetWorkflowIdRecord record, CancellationToken token)
    {
        await _client.SuspendTravelBookingAsync(new ApiTravel.SuspendTravelBookingRequest { Details = new ApiTravel.SuspendTravelBookingRequest.Types.Details { ReasonCode = record.ReferenceId } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.content.v1.ArticleQueryService.</summary>
internal sealed class RelayTimesheetWorkflowIdArticleQueryTarget(ApiContent.ArticleQueryService.ArticleQueryServiceClient client) : IRelayTimesheetWorkflowIdWriter
{
    public async Task ApplyAsync(RelayTimesheetWorkflowIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiContent.SuggestArticlesRequest { Filter = new ApiContent.SuggestArticlesRequest.Types.Filter { OwnerId = record.Notes }, OrderBy = record.ReferenceId }, token, client.SuggestArticlesAsync);
    }
}

/// <summary>Passes records to acme.sponsorships.v1.SponsorCalculationService.</summary>
internal sealed class RelayTimesheetWorkflowIdSponsorCalculationTarget : IRelayTimesheetWorkflowIdWriter
{
    private readonly ApiSponsorships.SponsorCalculationService.SponsorCalculationServiceClient _client;

    public RelayTimesheetWorkflowIdSponsorCalculationTarget(ApiSponsorships.SponsorCalculationService.SponsorCalculationServiceClient client) => _client = client;

    public async Task ApplyAsync(RelayTimesheetWorkflowIdRecord record, CancellationToken token)
    {
        await _client.RecalculateSponsorsAsync(new ApiSponsorships.RecalculateSponsorsRequest { Sponsor = new ApiSponsorships.Sponsor { SponsorId = record.ReferenceId } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class RelayTimesheetWorkflowIdLogOnly : IRelayTimesheetWorkflowIdWriter
{
    public Task ApplyAsync(RelayTimesheetWorkflowIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.ReferenceId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.timekeeping.v1.TimesheetWorkflowService/SignalTimesheetWorkflow and hands what it keeps to the sink of its kind.</summary>
internal static class RelayTimesheetWorkflowId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTimekeeping.TimesheetWorkflowService.TimesheetWorkflowServiceClient(channel);
        var got = await source.SignalTimesheetWorkflowAsync(new ApiTimekeeping.SignalTimesheetWorkflowRequest() { TimesheetId = "primary-relay-timesheet-workflow-id" }, cancellationToken: token);
        var record = new RelayTimesheetWorkflowIdRecord
        {
            ReferenceId = Text.Normalize(got.Outcome.ReferenceId),
            Notes = Text.Normalize(got.Timesheet.Notes),
        };
        await DeliverRelayTimesheetWorkflowIdAsync("article-query", record, channel, token);
    }

    private static async Task DeliverRelayTimesheetWorkflowIdAsync(string kind, RelayTimesheetWorkflowIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        IRelayTimesheetWorkflowIdWriter sink = new RelayTimesheetWorkflowIdLogOnly();
        switch (kind)
        {
            case "travel-booking-lifecycle":
                sink = new RelayTimesheetWorkflowIdTravelBookingLifecycleSink(new ApiTravel.TravelBookingLifecycleService.TravelBookingLifecycleServiceClient(channel));
                break;
            case "article-query":
                sink = new RelayTimesheetWorkflowIdArticleQueryTarget(new ApiContent.ArticleQueryService.ArticleQueryServiceClient(channel));
                break;
            case "sponsor-calculation":
                sink = new RelayTimesheetWorkflowIdSponsorCalculationTarget(new ApiSponsorships.SponsorCalculationService.SponsorCalculationServiceClient(channel));
                break;
        }
        await sink.ApplyAsync(record, token);
    }
}
