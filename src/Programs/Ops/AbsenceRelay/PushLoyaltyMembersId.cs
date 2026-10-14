using Grpc.Net.Client;
using ApiCompliance = Acme.Compliance.V1;
using ApiLoyalty = Acme.Loyalty.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.AbsenceRelay;

/// <summary>Where PushLoyaltyMembersId hands each record.</summary>
internal interface IPushLoyaltyMembersIdDestination
{
    Task SendAsync(PushLoyaltyMembersIdRecord record, CancellationToken token);
}

/// <summary>What PushLoyaltyMembersId keeps of each RecalculateLoyaltyMembers response.</summary>
internal sealed class PushLoyaltyMembersIdRecord
{
    public string LoyaltyMemberId { get; init; } = "";
    public string CurrencyCode { get; init; } = "";
}

/// <summary>Hands records to acme.workplaces.v1.VisitorPassSyncService.</summary>
internal sealed class PushLoyaltyMembersIdVisitorPassSyncWriter : IPushLoyaltyMembersIdDestination
{
    private readonly ApiWorkplaces.VisitorPassSyncService.VisitorPassSyncServiceClient _client;

    public PushLoyaltyMembersIdVisitorPassSyncWriter(ApiWorkplaces.VisitorPassSyncService.VisitorPassSyncServiceClient client) => _client = client;

    public async Task SendAsync(PushLoyaltyMembersIdRecord record, CancellationToken token)
    {
        await _client.ReconcileVisitorPassesAsync(new ApiWorkplaces.ReconcileVisitorPassesRequest { VisitorPassId = record.LoyaltyMemberId }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.compliance.v1.ComplianceCaseService.</summary>
internal sealed class PushLoyaltyMembersIdComplianceCaseTarget(ApiCompliance.ComplianceCaseService.ComplianceCaseServiceClient client) : IPushLoyaltyMembersIdDestination
{
    public async Task SendAsync(PushLoyaltyMembersIdRecord record, CancellationToken token)
    {
        await client.CloneComplianceCaseAsync(new ApiCompliance.CloneComplianceCaseRequest { ComplianceCase = new ApiCompliance.ComplianceCase { ComplianceCaseId = record.CurrencyCode } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class PushLoyaltyMembersIdLogOnly : IPushLoyaltyMembersIdDestination
{
    public Task SendAsync(PushLoyaltyMembersIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.LoyaltyMemberId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.loyalty.v1.LoyaltyMemberCalculationService/RecalculateLoyaltyMembers and hands what it keeps to the sink of its kind.</summary>
internal static class PushLoyaltyMembersId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiLoyalty.LoyaltyMemberCalculationService.LoyaltyMemberCalculationServiceClient(channel);
        var reply = await source.RecalculateLoyaltyMembersAsync(new ApiLoyalty.RecalculateLoyaltyMembersRequest() { Reason = "nightly-push-loyalty-members-id" }, cancellationToken: token);
        var record = new PushLoyaltyMembersIdRecord
        {
            LoyaltyMemberId = reply.LoyaltyMember.LoyaltyMemberId,
            CurrencyCode = reply.Breakdown.CurrencyCode,
        };
        await DeliverPushLoyaltyMembersIdAsync("compliance-case", record, channel, token);
    }

    private static async Task DeliverPushLoyaltyMembersIdAsync(string kind, PushLoyaltyMembersIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        IPushLoyaltyMembersIdDestination sink = new PushLoyaltyMembersIdLogOnly();
        switch (kind)
        {
            case "visitor-pass-sync":
                sink = new PushLoyaltyMembersIdVisitorPassSyncWriter(new ApiWorkplaces.VisitorPassSyncService.VisitorPassSyncServiceClient(channel));
                break;
            case "compliance-case":
                sink = new PushLoyaltyMembersIdComplianceCaseTarget(new ApiCompliance.ComplianceCaseService.ComplianceCaseServiceClient(channel));
                break;
        }
        await sink.SendAsync(record, token);
    }
}
