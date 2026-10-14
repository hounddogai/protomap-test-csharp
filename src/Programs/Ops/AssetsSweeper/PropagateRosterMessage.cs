using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiSellers = Acme.Sellers.V1;

namespace Acme.Workforce.Ops.AssetsSweeper;

/// <summary>Where PropagateRosterMessage hands each record.</summary>
internal interface IPropagateRosterMessageDestination
{
    Task SendAsync(PropagateRosterMessageRecord record, CancellationToken token);
}

/// <summary>What PropagateRosterMessage keeps of each SubmitRoster response.</summary>
internal sealed class PropagateRosterMessageRecord
{
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
    public string OutcomeStatus { get; init; } = "";
    public string OutcomeMessage { get; init; } = "";
}

/// <summary>Hands records to acme.itsm.v1.ServiceAssetApprovalService.</summary>
internal sealed class PropagateRosterMessageServiceAssetApprovalWriter : IPropagateRosterMessageDestination
{
    private readonly ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient _client;

    public PropagateRosterMessageServiceAssetApprovalWriter(ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient client) => _client = client;

    public async Task SendAsync(PropagateRosterMessageRecord record, CancellationToken token)
    {
        await _client.SubmitServiceAssetAsync(new ApiItsm.SubmitServiceAssetRequest { Details = new ApiItsm.SubmitServiceAssetRequest.Types.Details { ApproverEmail = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.employeeportal.v1.PortalHomeIntegrationService.</summary>
internal sealed class PropagateRosterMessagePortalHomeIntegrationSink(ApiEmployeeportal.PortalHomeIntegrationService.PortalHomeIntegrationServiceClient client) : IPropagateRosterMessageDestination
{
    public async Task SendAsync(PropagateRosterMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiEmployeeportal.ReplayPortalHomeEventsRequest { Reason = record.Message, Details = new ApiEmployeeportal.ReplayPortalHomeEventsRequest.Types.Details { MessageId = record.OutcomeStatus, SourceSystem = record.Status } }, token, client.ReplayPortalHomeEventsAsync);
    }
}

/// <summary>Passes records to acme.sellers.v1.SellerCalculationService.</summary>
internal sealed class PropagateRosterMessageSellerCalculationWriter(ApiSellers.SellerCalculationService.SellerCalculationServiceClient client) : IPropagateRosterMessageDestination
{
    public async Task SendAsync(PropagateRosterMessageRecord record, CancellationToken token)
    {
        await client.GetSellerRatesAsync(new ApiSellers.GetSellerRatesRequest { SellerId = record.OutcomeStatus }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class PropagateRosterMessageLogOnly : IPropagateRosterMessageDestination
{
    public Task SendAsync(PropagateRosterMessageRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Message}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.scheduling.v1.RosterApprovalService/SubmitRoster and hands what it keeps to the sink of its kind.</summary>
internal static class PropagateRosterMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiScheduling.RosterApprovalService.RosterApprovalServiceClient(channel);
        var found = await source.SubmitRosterAsync(new ApiScheduling.SubmitRosterRequest() { Reason = "default-propagate-roster-message" }, cancellationToken: token);
        var record = new PropagateRosterMessageRecord
        {
            Message = found.Result.Message,
            Status = found.Result.Status,
            OutcomeStatus = found.Outcome.Status,
            OutcomeMessage = found.Outcome.Message,
        };
        await DeliverPropagateRosterMessageAsync("portal-home-integration", record, channel, token);
    }

    private static async Task DeliverPropagateRosterMessageAsync(string kind, PropagateRosterMessageRecord record, GrpcChannel channel, CancellationToken token)
    {
        IPropagateRosterMessageDestination sink = kind switch
        {
            "asset-approval" => new PropagateRosterMessageServiceAssetApprovalWriter(new ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient(channel)),
            "portal-home-integration" => new PropagateRosterMessagePortalHomeIntegrationSink(new ApiEmployeeportal.PortalHomeIntegrationService.PortalHomeIntegrationServiceClient(channel)),
            "seller-calculation" => new PropagateRosterMessageSellerCalculationWriter(new ApiSellers.SellerCalculationService.SellerCalculationServiceClient(channel)),
            _ => new PropagateRosterMessageLogOnly(),
        };
        await sink.SendAsync(record, token);
    }
}
