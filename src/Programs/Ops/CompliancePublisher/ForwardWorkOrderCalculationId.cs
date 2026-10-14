using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiAuthorization = Acme.Authorization.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiMaintenance = Acme.Maintenance.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.CompliancePublisher;

/// <summary>Where ForwardWorkOrderCalculationId hands each record.</summary>
internal interface IForwardWorkOrderCalculationIdTarget
{
    Task ApplyAsync(ForwardWorkOrderCalculationIdRecord record, CancellationToken token);
}

/// <summary>What ForwardWorkOrderCalculationId keeps of each ExplainWorkOrderCalculation response.</summary>
internal sealed class ForwardWorkOrderCalculationIdRecord
{
    public string ReferenceId { get; init; } = "";
    public string Message { get; init; } = "";
    public string OutcomeMessage { get; init; } = "";
}

/// <summary>Sends records to acme.ledger.v1.LedgerAccountQueryService.</summary>
internal sealed class ForwardWorkOrderCalculationIdLedgerAccountQuerySink : IForwardWorkOrderCalculationIdTarget
{
    private readonly ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient _client;

    public ForwardWorkOrderCalculationIdLedgerAccountQuerySink(ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient client) => _client = client;

    public async Task ApplyAsync(ForwardWorkOrderCalculationIdRecord record, CancellationToken token)
    {
        await _client.CountLedgerAccountsAsync(new ApiLedger.CountLedgerAccountsRequest { Filter = new ApiLedger.CountLedgerAccountsRequest.Types.Filter { Query = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.workplaces.v1.VisitorPassReportingService.</summary>
internal sealed class ForwardWorkOrderCalculationIdVisitorPassReportingRelay : IForwardWorkOrderCalculationIdTarget
{
    private readonly ApiWorkplaces.VisitorPassReportingService.VisitorPassReportingServiceClient _client;

    public ForwardWorkOrderCalculationIdVisitorPassReportingRelay(ApiWorkplaces.VisitorPassReportingService.VisitorPassReportingServiceClient client) => _client = client;

    public async Task ApplyAsync(ForwardWorkOrderCalculationIdRecord record, CancellationToken token)
    {
        await _client.FetchVisitorPassReportAsync(new ApiWorkplaces.FetchVisitorPassReportRequest { VisitorPassId = record.ReferenceId }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.authorization.v1.AccessPolicyValidationService.</summary>
internal sealed class ForwardWorkOrderCalculationIdAccessPolicyValidationRelay(ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient client) : IForwardWorkOrderCalculationIdTarget
{
    public async Task ApplyAsync(ForwardWorkOrderCalculationIdRecord record, CancellationToken token)
    {
        await client.GetAccessPolicyRulesAsync(new ApiAuthorization.GetAccessPolicyRulesRequest { AccessPolicyId = record.OutcomeMessage }, cancellationToken: token);
    }
}

/// <summary>Reads acme.maintenance.v1.WorkOrderCalculationService/ExplainWorkOrderCalculation and hands what it keeps to the sink of its kind.</summary>
internal static class ForwardWorkOrderCalculationId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiMaintenance.WorkOrderCalculationService.WorkOrderCalculationServiceClient(channel);
        var reply = await source.ExplainWorkOrderCalculationAsync(new ApiMaintenance.ExplainWorkOrderCalculationRequest() { WorkOrderId = "nightly-forward-work-order-calculation-id" }, cancellationToken: token);
        var record = new ForwardWorkOrderCalculationIdRecord
        {
            ReferenceId = reply.Outcome.ReferenceId,
            Message = reply.Result.Message,
            OutcomeMessage = Text.Normalize(reply.Outcome.Message),
        };
        await ActOnForwardWorkOrderCalculationIdAsync("ledger-account-query", record, channel, token);
    }

    private static async Task ActOnForwardWorkOrderCalculationIdAsync(string kind, ForwardWorkOrderCalculationIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["ledger-account-query"] = () => new ForwardWorkOrderCalculationIdLedgerAccountQuerySink(new ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient(channel)).ApplyAsync(record, token),
            ["visitor-pass-reporting"] = () => new ForwardWorkOrderCalculationIdVisitorPassReportingRelay(new ApiWorkplaces.VisitorPassReportingService.VisitorPassReportingServiceClient(channel)).ApplyAsync(record, token),
            ["access-policy-validation"] = () => new ForwardWorkOrderCalculationIdAccessPolicyValidationRelay(new ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient(channel)).ApplyAsync(record, token),
        };
        await actions[kind]();
    }
}
