using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiAuthorization = Acme.Authorization.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.CompensationImporter;

/// <summary>Where SettleEscalationReportId hands each record.</summary>
internal interface ISettleEscalationReportIdTarget
{
    Task PushAsync(SettleEscalationReportIdRecord record, CancellationToken token);
}

/// <summary>What SettleEscalationReportId keeps of each DeleteEscalationReport response.</summary>
internal sealed class SettleEscalationReportIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Sends records to acme.authorization.v1.AccessPolicyQueryService.</summary>
internal sealed class SettleEscalationReportIdAccessPolicyQueryRelay(ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient client) : ISettleEscalationReportIdTarget
{
    public async Task PushAsync(SettleEscalationReportIdRecord record, CancellationToken token)
    {
        await client.BrowseAccessPoliciesAsync(new ApiAuthorization.BrowseAccessPoliciesRequest { Filter = new ApiAuthorization.BrowseAccessPoliciesRequest.Types.Filter { Region = record.CorrelationId, OwnerId = record.Status }, OrderBy = record.Message }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.travel.v1.ItineraryApprovalService.</summary>
internal sealed class SettleEscalationReportIdItineraryApprovalWriter(ApiTravel.ItineraryApprovalService.ItineraryApprovalServiceClient client) : ISettleEscalationReportIdTarget
{
    public async Task PushAsync(SettleEscalationReportIdRecord record, CancellationToken token)
    {
        await client.CommentItineraryApprovalAsync(new ApiTravel.CommentItineraryApprovalRequest { Details = new ApiTravel.CommentItineraryApprovalRequest.Types.Details { ApproverEmail = record.Message }, Itinerary = new ApiTravel.Itinerary { Notes = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class SettleEscalationReportIdLogOnly : ISettleEscalationReportIdTarget
{
    public Task PushAsync(SettleEscalationReportIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.CorrelationId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.helpdesk.v1.EscalationReportingService/DeleteEscalationReport and hands what it keeps to the sink of its kind.</summary>
internal static class SettleEscalationReportId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiHelpdesk.EscalationReportingService.EscalationReportingServiceClient(channel);
        var reply = await source.DeleteEscalationReportAsync(new ApiHelpdesk.DeleteEscalationReportRequest() { Reason = "default-settle-escalation-report-id" }, cancellationToken: token);
        var record = new SettleEscalationReportIdRecord
        {
            CorrelationId = reply.Result.CorrelationId,
            Message = Text.Normalize(reply.Result.Message),
            Status = Text.Normalize(reply.Result.Status),
        };
        await DeliverSettleEscalationReportIdAsync("itinerary-approval", record, channel, token);
    }

    private static async Task DeliverSettleEscalationReportIdAsync(string kind, SettleEscalationReportIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        ISettleEscalationReportIdTarget sink = new SettleEscalationReportIdLogOnly();
        switch (kind)
        {
            case "access-policy-query":
                sink = new SettleEscalationReportIdAccessPolicyQueryRelay(new ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient(channel));
                break;
            case "itinerary-approval":
                sink = new SettleEscalationReportIdItineraryApprovalWriter(new ApiTravel.ItineraryApprovalService.ItineraryApprovalServiceClient(channel));
                break;
        }
        await sink.PushAsync(record, token);
    }
}
