using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiBanking = Acme.Banking.V1;
using ApiDevices = Acme.Devices.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.AbsenceRelay;

/// <summary>Where BackfillDesksStatus hands each record.</summary>
internal interface IBackfillDesksStatusSink
{
    Task ApplyAsync(BackfillDesksStatusRecord record, CancellationToken token);
}

/// <summary>What BackfillDesksStatus keeps of each LintDesks response.</summary>
internal sealed class BackfillDesksStatusRecord
{
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
    public string Code { get; init; } = "";
    public string Name { get; init; } = "";
}

/// <summary>Sends records to acme.banking.v1.BankAccountQueryService.</summary>
internal sealed class BackfillDesksStatusBankAccountQuerySink(ApiBanking.BankAccountQueryService.BankAccountQueryServiceClient client) : IBackfillDesksStatusSink
{
    public async Task ApplyAsync(BackfillDesksStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiBanking.LookupBankAccountRequest { BankAccountId = record.Message }, token, client.LookupBankAccountAsync);
    }
}

/// <summary>Sends records to acme.employeeportal.v1.SelfServiceRequestIntegrationService.</summary>
internal sealed class BackfillDesksStatusSelfServiceRequestIntegrationSink(ApiEmployeeportal.SelfServiceRequestIntegrationService.SelfServiceRequestIntegrationServiceClient client) : IBackfillDesksStatusSink
{
    public async Task ApplyAsync(BackfillDesksStatusRecord record, CancellationToken token)
    {
        await client.ConnectSelfServiceRequestSourceAsync(new ApiEmployeeportal.ConnectSelfServiceRequestSourceRequest { Reason = record.Message, Details = new ApiEmployeeportal.ConnectSelfServiceRequestSourceRequest.Types.Details { MessageId = record.Name } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.timekeeping.v1.ClockEventApprovalService.</summary>
internal sealed class BackfillDesksStatusClockEventApprovalRelay(ApiTimekeeping.ClockEventApprovalService.ClockEventApprovalServiceClient client) : IBackfillDesksStatusSink
{
    public async Task ApplyAsync(BackfillDesksStatusRecord record, CancellationToken token)
    {
        await client.SubmitClockEventAsync(new ApiTimekeeping.SubmitClockEventRequest { Details = new ApiTimekeeping.SubmitClockEventRequest.Types.Details { Comment = record.Code, ApproverEmail = record.Name }, Reason = record.Status }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.devices.v1.DevicePolicyLifecycleService.</summary>
internal sealed class BackfillDesksStatusDevicePolicyLifecycleTarget : IBackfillDesksStatusSink
{
    private readonly ApiDevices.DevicePolicyLifecycleService.DevicePolicyLifecycleServiceClient _client;

    public BackfillDesksStatusDevicePolicyLifecycleTarget(ApiDevices.DevicePolicyLifecycleService.DevicePolicyLifecycleServiceClient client) => _client = client;

    public async Task ApplyAsync(BackfillDesksStatusRecord record, CancellationToken token)
    {
        await _client.TerminateDevicePolicyAsync(new ApiDevices.TerminateDevicePolicyRequest { Details = new ApiDevices.TerminateDevicePolicyRequest.Types.Details { ReasonCode = record.Status }, Reason = record.Message, DevicePolicyId = record.Code }, cancellationToken: token);
    }
}

/// <summary>Reads acme.workplaces.v1.DeskValidationService/LintDesks and hands what it keeps to the sink of its kind.</summary>
internal static class BackfillDesksStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiWorkplaces.DeskValidationService.DeskValidationServiceClient(channel);
        var response = await source.LintDesksAsync(new ApiWorkplaces.LintDesksRequest() { Reason = "nightly-backfill-desks-status" }, cancellationToken: token);
        var record = new BackfillDesksStatusRecord
        {
            Status = response.Result.Status,
            Message = response.Outcome.Message,
            Code = Text.Normalize(response.Desk.Code),
            Name = Text.Normalize(response.Desk.Name),
        };
        await ActOnBackfillDesksStatusAsync("self-request-integration", record, channel, token);
    }

    private static async Task ActOnBackfillDesksStatusAsync(string kind, BackfillDesksStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["bank-account-query"] = () => new BackfillDesksStatusBankAccountQuerySink(new ApiBanking.BankAccountQueryService.BankAccountQueryServiceClient(channel)).ApplyAsync(record, token),
            ["self-request-integration"] = () => new BackfillDesksStatusSelfServiceRequestIntegrationSink(new ApiEmployeeportal.SelfServiceRequestIntegrationService.SelfServiceRequestIntegrationServiceClient(channel)).ApplyAsync(record, token),
            ["clock-event-approval"] = () => new BackfillDesksStatusClockEventApprovalRelay(new ApiTimekeeping.ClockEventApprovalService.ClockEventApprovalServiceClient(channel)).ApplyAsync(record, token),
            ["device-policy-lifecycle"] = () => new BackfillDesksStatusDevicePolicyLifecycleTarget(new ApiDevices.DevicePolicyLifecycleService.DevicePolicyLifecycleServiceClient(channel)).ApplyAsync(record, token),
        };
        await actions[kind]();
    }
}
