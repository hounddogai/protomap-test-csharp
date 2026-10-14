using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiLedger = Acme.Ledger.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Ops.PayrollopsRelay;

/// <summary>Where CopyLedgerAccountCode hands each record.</summary>
internal interface ICopyLedgerAccountCodeTarget
{
    Task ForwardAsync(CopyLedgerAccountCodeRecord record, CancellationToken token);
}

/// <summary>What CopyLedgerAccountCode keeps of each PeekLedgerAccount response.</summary>
internal sealed class CopyLedgerAccountCodeRecord
{
    public string CurrencyCode { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Sends records to acme.onboarding.v1.EquipmentLifecycleService.</summary>
internal sealed class CopyLedgerAccountCodeEquipmentLifecycleSink : ICopyLedgerAccountCodeTarget
{
    private readonly ApiOnboarding.EquipmentLifecycleService.EquipmentLifecycleServiceClient _client;

    public CopyLedgerAccountCodeEquipmentLifecycleSink(ApiOnboarding.EquipmentLifecycleService.EquipmentLifecycleServiceClient client) => _client = client;

    public async Task ForwardAsync(CopyLedgerAccountCodeRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiOnboarding.ExpireEquipmentRequest { Details = new ApiOnboarding.ExpireEquipmentRequest.Types.Details { ReasonCode = record.Message } }, token, _client.ExpireEquipmentAsync);
    }
}

/// <summary>Passes records to acme.timekeeping.v1.TimesheetWorkflowService.</summary>
internal sealed class CopyLedgerAccountCodeTimesheetWorkflowWriter(ApiTimekeeping.TimesheetWorkflowService.TimesheetWorkflowServiceClient client) : ICopyLedgerAccountCodeTarget
{
    public async Task ForwardAsync(CopyLedgerAccountCodeRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTimekeeping.AdvanceTimesheetWorkflowRequest { Details = new ApiTimekeeping.AdvanceTimesheetWorkflowRequest.Types.Details { InitiatedBy = record.Message }, TimesheetId = record.CorrelationId, Timesheet = new ApiTimekeeping.Timesheet { Number = record.Status } }, token, client.AdvanceTimesheetWorkflowAsync);
    }
}

/// <summary>Reads acme.ledger.v1.LedgerAccountQueryService/PeekLedgerAccount and hands what it keeps to the sink of its kind.</summary>
internal static class CopyLedgerAccountCode
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient(channel);
        var reply = await source.PeekLedgerAccountAsync(new ApiLedger.PeekLedgerAccountRequest() { LedgerAccountId = "current-copy-ledger-account-code" }, cancellationToken: token);
        var record = new CopyLedgerAccountCodeRecord
        {
            CurrencyCode = reply.LedgerAccount.CurrencyCode,
            CorrelationId = reply.Result.CorrelationId,
            Status = reply.Result.Status,
            Message = reply.Result.Message,
        };
        await ActOnCopyLedgerAccountCodeAsync("timesheet-workflow", record, channel, token);
    }

    private static async Task ActOnCopyLedgerAccountCodeAsync(string kind, CopyLedgerAccountCodeRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["equipment-lifecycle"] = () => new CopyLedgerAccountCodeEquipmentLifecycleSink(new ApiOnboarding.EquipmentLifecycleService.EquipmentLifecycleServiceClient(channel)).ForwardAsync(record, token),
            ["timesheet-workflow"] = () => new CopyLedgerAccountCodeTimesheetWorkflowWriter(new ApiTimekeeping.TimesheetWorkflowService.TimesheetWorkflowServiceClient(channel)).ForwardAsync(record, token),
        };
        await actions[kind]();
    }
}
