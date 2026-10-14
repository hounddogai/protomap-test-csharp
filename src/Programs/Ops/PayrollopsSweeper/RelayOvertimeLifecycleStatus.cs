using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiAuthorization = Acme.Authorization.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiTelematics = Acme.Telematics.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Ops.PayrollopsSweeper;

/// <summary>Where RelayOvertimeLifecycleStatus hands each record.</summary>
internal interface IRelayOvertimeLifecycleStatusSink
{
    Task SendAsync(RelayOvertimeLifecycleStatusRecord record, CancellationToken token);
}

/// <summary>What RelayOvertimeLifecycleStatus keeps of each GetOvertimeLifecycle response.</summary>
internal sealed class RelayOvertimeLifecycleStatusRecord
{
    public string Status { get; init; } = "";
    public string Description { get; init; } = "";
}

/// <summary>Hands records to acme.authorization.v1.AccessPolicyValidationService.</summary>
internal sealed class RelayOvertimeLifecycleStatusAccessPolicyValidationSink : IRelayOvertimeLifecycleStatusSink
{
    private readonly ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient _client;

    public RelayOvertimeLifecycleStatusAccessPolicyValidationSink(ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient client) => _client = client;

    public async Task SendAsync(RelayOvertimeLifecycleStatusRecord record, CancellationToken token)
    {
        await _client.ScoreAccessPolicyAsync(new ApiAuthorization.ScoreAccessPolicyRequest { Reason = record.Description }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.telematics.v1.TelemetryReadingCalculationService.</summary>
internal sealed class RelayOvertimeLifecycleStatusTelemetryReadingCalculationRelay : IRelayOvertimeLifecycleStatusSink
{
    private readonly ApiTelematics.TelemetryReadingCalculationService.TelemetryReadingCalculationServiceClient _client;

    public RelayOvertimeLifecycleStatusTelemetryReadingCalculationRelay(ApiTelematics.TelemetryReadingCalculationService.TelemetryReadingCalculationServiceClient client) => _client = client;

    public async Task SendAsync(RelayOvertimeLifecycleStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTelematics.CalculateTelemetryReadingRequest { Details = new ApiTelematics.CalculateTelemetryReadingRequest.Types.Details { CurrencyCode = record.Status }, TelemetryReading = new ApiTelematics.TelemetryReading { TelemetryReadingId = record.Description } }, token, _client.CalculateTelemetryReadingAsync);
    }
}

/// <summary>Writes records to acme.itsm.v1.ProblemApprovalService.</summary>
internal sealed class RelayOvertimeLifecycleStatusProblemApprovalTarget(ApiItsm.ProblemApprovalService.ProblemApprovalServiceClient client) : IRelayOvertimeLifecycleStatusSink
{
    public async Task SendAsync(RelayOvertimeLifecycleStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiItsm.DelegateProblemApprovalRequest { Details = new ApiItsm.DelegateProblemApprovalRequest.Types.Details { Comment = record.Description } }, token, client.DelegateProblemApprovalAsync);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class RelayOvertimeLifecycleStatusLogOnly : IRelayOvertimeLifecycleStatusSink
{
    public Task SendAsync(RelayOvertimeLifecycleStatusRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Status}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.timekeeping.v1.OvertimeLifecycleService/GetOvertimeLifecycle and hands what it keeps to the sink of its kind.</summary>
internal static class RelayOvertimeLifecycleStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTimekeeping.OvertimeLifecycleService.OvertimeLifecycleServiceClient(channel);
        var reply = await source.GetOvertimeLifecycleAsync(new ApiTimekeeping.GetOvertimeLifecycleRequest() { OvertimeId = "default-relay-overtime-lifecycle-status" }, cancellationToken: token);
        var record = new RelayOvertimeLifecycleStatusRecord
        {
            Status = Text.Normalize(reply.Result.Status),
            Description = reply.Overtime.Description,
        };
        await DispatchRelayOvertimeLifecycleStatusAsync("access-policy-validation", record, channel, token);
    }

    private static async Task DispatchRelayOvertimeLifecycleStatusAsync(string kind, RelayOvertimeLifecycleStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<RelayOvertimeLifecycleStatusRecord, CancellationToken, Task> write = new RelayOvertimeLifecycleStatusLogOnly().SendAsync;
        switch (kind)
        {
            case "access-policy-validation":
                write = new RelayOvertimeLifecycleStatusAccessPolicyValidationSink(new ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient(channel)).SendAsync;
                break;
            case "telemetry-reading-calculation":
                write = new RelayOvertimeLifecycleStatusTelemetryReadingCalculationRelay(new ApiTelematics.TelemetryReadingCalculationService.TelemetryReadingCalculationServiceClient(channel)).SendAsync;
                break;
            case "problem-approval":
                write = new RelayOvertimeLifecycleStatusProblemApprovalTarget(new ApiItsm.ProblemApprovalService.ProblemApprovalServiceClient(channel)).SendAsync;
                break;
        }
        await write(record, token);
    }
}
