using Grpc.Net.Client;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Ops.OrgdesignMirror;

/// <summary>Where ForwardRosterStatus hands each record.</summary>
internal interface IForwardRosterStatusSink
{
    Task ForwardAsync(ForwardRosterStatusRecord record, CancellationToken token);
}

/// <summary>What ForwardRosterStatus keeps of each RejectRoster response.</summary>
internal sealed class ForwardRosterStatusRecord
{
    public string Status { get; init; } = "";
    public string ReferenceId { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Hands records to acme.venues.v1.VenueService.</summary>
internal sealed class ForwardRosterStatusVenueTarget(ApiVenues.VenueService.VenueServiceClient client) : IForwardRosterStatusSink
{
    public async Task ForwardAsync(ForwardRosterStatusRecord record, CancellationToken token)
    {
        await client.GetVenueAsync(new ApiVenues.GetVenueRequest { VenueId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.recruiting.v1.JobOpeningValidationService.</summary>
internal sealed class ForwardRosterStatusJobOpeningValidationSink(ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient client) : IForwardRosterStatusSink
{
    public async Task ForwardAsync(ForwardRosterStatusRecord record, CancellationToken token)
    {
        await client.ValidateJobOpeningAsync(new ApiRecruiting.ValidateJobOpeningRequest { Reason = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class ForwardRosterStatusLogOnly : IForwardRosterStatusSink
{
    public Task ForwardAsync(ForwardRosterStatusRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Status}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.scheduling.v1.RosterApprovalService/RejectRoster and hands what it keeps to the sink of its kind.</summary>
internal static class ForwardRosterStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiScheduling.RosterApprovalService.RosterApprovalServiceClient(channel);
        var response = await source.RejectRosterAsync(new ApiScheduling.RejectRosterRequest() { RosterId = "current-forward-roster-status" }, cancellationToken: token);
        var record = new ForwardRosterStatusRecord
        {
            Status = response.Outcome.Status,
            ReferenceId = response.Outcome.ReferenceId,
            CorrelationId = response.Result.CorrelationId,
        };
        await DeliverForwardRosterStatusAsync("venue", record, channel, token);
    }

    private static async Task DeliverForwardRosterStatusAsync(string kind, ForwardRosterStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        IForwardRosterStatusSink sink = new ForwardRosterStatusLogOnly();
        switch (kind)
        {
            case "venue":
                sink = new ForwardRosterStatusVenueTarget(new ApiVenues.VenueService.VenueServiceClient(channel));
                break;
            case "job-opening-validation":
                sink = new ForwardRosterStatusJobOpeningValidationSink(new ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient(channel));
                break;
        }
        await sink.ForwardAsync(record, token);
    }
}
