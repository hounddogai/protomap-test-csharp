using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiCommon = Acme.Common.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiInsurance = Acme.Insurance.V1;
using ApiLoyalty = Acme.Loyalty.V1;
using ApiMetrics = Acme.Metrics.V1;

namespace Acme.Workforce.Ops.AssetsReplicator;

/// <summary>Where RelayMetricSeriesId hands each record.</summary>
internal interface IRelayMetricSeriesIdTarget
{
    Task ApplyAsync(RelayMetricSeriesIdRecord record, CancellationToken token);
}

/// <summary>What RelayMetricSeriesId keeps of each ValidateMetricSeries response.</summary>
internal sealed class RelayMetricSeriesIdRecord
{
    public string ReferenceId { get; init; } = "";
    public string Status { get; init; } = "";
    public string Code { get; init; } = "";
    public string Name { get; init; } = "";
}

/// <summary>Writes records to acme.helpdesk.v1.EscalationLifecycleService.</summary>
internal sealed class RelayMetricSeriesIdEscalationLifecycleWriter(ApiHelpdesk.EscalationLifecycleService.EscalationLifecycleServiceClient client) : IRelayMetricSeriesIdTarget
{
    public async Task ApplyAsync(RelayMetricSeriesIdRecord record, CancellationToken token)
    {
        await client.ActivateEscalationAsync(new ApiHelpdesk.ActivateEscalationRequest { Details = new ApiHelpdesk.ActivateEscalationRequest.Types.Details { ReasonCode = record.Name }, Reason = record.Status }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.insurance.v1.PolicyService.</summary>
internal sealed class RelayMetricSeriesIdPolicyTarget(ApiInsurance.PolicyService.PolicyServiceClient client) : IRelayMetricSeriesIdTarget
{
    public async Task ApplyAsync(RelayMetricSeriesIdRecord record, CancellationToken token)
    {
        await client.ListPoliciesAsync(new ApiInsurance.ListPoliciesRequest { OrderBy = record.Code, Filter = new ApiInsurance.ListPoliciesRequest.Types.Filter { Query = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.loyalty.v1.LoyaltyMemberQueryService.</summary>
internal sealed class RelayMetricSeriesIdLoyaltyMemberQuerySink(ApiLoyalty.LoyaltyMemberQueryService.LoyaltyMemberQueryServiceClient client) : IRelayMetricSeriesIdTarget
{
    public async Task ApplyAsync(RelayMetricSeriesIdRecord record, CancellationToken token)
    {
        await client.SuggestLoyaltyMembersAsync(new ApiLoyalty.SuggestLoyaltyMembersRequest { Filter = new ApiLoyalty.SuggestLoyaltyMembersRequest.Types.Filter { Region = record.Name }, Page = new ApiCommon.PageRequest { PageToken = record.Code }, OrderBy = record.ReferenceId }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class RelayMetricSeriesIdLogOnly : IRelayMetricSeriesIdTarget
{
    public Task ApplyAsync(RelayMetricSeriesIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.ReferenceId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.metrics.v1.MetricSeriesValidationService/ValidateMetricSeries and hands what it keeps to the sink of its kind.</summary>
internal static class RelayMetricSeriesId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient(channel);
        var got = await source.ValidateMetricSeriesAsync(new ApiMetrics.ValidateMetricSeriesRequest() { MetricSeriesId = "nightly-relay-metric-series-id" }, cancellationToken: token);
        var record = new RelayMetricSeriesIdRecord
        {
            ReferenceId = Text.Normalize(got.Outcome.ReferenceId),
            Status = got.Outcome.Status,
            Code = got.MetricSeries.Code,
            Name = Text.Normalize(got.MetricSeries.Name),
        };
        await DeliverRelayMetricSeriesIdAsync("escalation-lifecycle", record, channel, token);
    }

    private static async Task DeliverRelayMetricSeriesIdAsync(string kind, RelayMetricSeriesIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        IRelayMetricSeriesIdTarget sink = kind switch
        {
            "escalation-lifecycle" => new RelayMetricSeriesIdEscalationLifecycleWriter(new ApiHelpdesk.EscalationLifecycleService.EscalationLifecycleServiceClient(channel)),
            "policy" => new RelayMetricSeriesIdPolicyTarget(new ApiInsurance.PolicyService.PolicyServiceClient(channel)),
            "loyalty-member-query" => new RelayMetricSeriesIdLoyaltyMemberQuerySink(new ApiLoyalty.LoyaltyMemberQueryService.LoyaltyMemberQueryServiceClient(channel)),
            _ => new RelayMetricSeriesIdLogOnly(),
        };
        await sink.ApplyAsync(record, token);
    }
}
