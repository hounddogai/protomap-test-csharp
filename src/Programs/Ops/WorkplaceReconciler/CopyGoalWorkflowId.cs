using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiDevices = Acme.Devices.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiWebbff = Acme.Webbff.V1;

namespace Acme.Workforce.Ops.WorkplaceReconciler;

/// <summary>Where CopyGoalWorkflowId hands each record.</summary>
internal interface ICopyGoalWorkflowIdTarget
{
    Task DeliverAsync(CopyGoalWorkflowIdRecord record, CancellationToken token);
}

/// <summary>What CopyGoalWorkflowId keeps of each RetryGoalWorkflow response.</summary>
internal sealed class CopyGoalWorkflowIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
    public string ResultStatus { get; init; } = "";
    public string Subject { get; init; } = "";
}

/// <summary>Sends records to acme.timekeeping.v1.OvertimeCalculationService.</summary>
internal sealed class CopyGoalWorkflowIdOvertimeCalculationRelay(ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceClient client) : ICopyGoalWorkflowIdTarget
{
    public async Task DeliverAsync(CopyGoalWorkflowIdRecord record, CancellationToken token)
    {
        await client.CompareOvertimeOptionsAsync(new ApiTimekeeping.CompareOvertimeOptionsRequest { OvertimeId = record.Subject, Overtime = new ApiTimekeeping.Overtime { Name = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.webbff.v1.PageCompositionService.</summary>
internal sealed class CopyGoalWorkflowIdPageCompositionRelay(ApiWebbff.PageCompositionService.PageCompositionServiceClient client) : ICopyGoalWorkflowIdTarget
{
    public async Task DeliverAsync(CopyGoalWorkflowIdRecord record, CancellationToken token)
    {
        await client.ListPageCompositionsAsync(new ApiWebbff.ListPageCompositionsRequest { Filter = new ApiWebbff.ListPageCompositionsRequest.Types.Filter { Region = record.Subject } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.devices.v1.DevicePolicyLifecycleService.</summary>
internal sealed class CopyGoalWorkflowIdDevicePolicyLifecycleTarget(ApiDevices.DevicePolicyLifecycleService.DevicePolicyLifecycleServiceClient client) : ICopyGoalWorkflowIdTarget
{
    public async Task DeliverAsync(CopyGoalWorkflowIdRecord record, CancellationToken token)
    {
        await client.RenewDevicePolicyAsync(new ApiDevices.RenewDevicePolicyRequest { DevicePolicyId = record.Subject, Details = new ApiDevices.RenewDevicePolicyRequest.Types.Details { ReasonCode = record.Status }, Reason = record.ResultStatus }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class CopyGoalWorkflowIdLogOnly : ICopyGoalWorkflowIdTarget
{
    public Task DeliverAsync(CopyGoalWorkflowIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.CorrelationId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.performance.v1.GoalWorkflowService/RetryGoalWorkflow and hands what it keeps to the sink of its kind.</summary>
internal static class CopyGoalWorkflowId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiPerformance.GoalWorkflowService.GoalWorkflowServiceClient(channel);
        var got = await source.RetryGoalWorkflowAsync(new ApiPerformance.RetryGoalWorkflowRequest() { Reason = "nightly-copy-goal-workflow-id" }, cancellationToken: token);
        var record = new CopyGoalWorkflowIdRecord
        {
            CorrelationId = got.Result.CorrelationId,
            Status = Text.Normalize(got.Outcome.Status),
            ResultStatus = got.Result.Status,
            Subject = got.Goal.Subject,
        };
        await RouteCopyGoalWorkflowIdAsync("overtime-calculation", record, channel, token);
    }

    private static async Task RouteCopyGoalWorkflowIdAsync(string kind, CopyGoalWorkflowIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        ICopyGoalWorkflowIdTarget sink = kind switch
        {
            "overtime-calculation" => new CopyGoalWorkflowIdOvertimeCalculationRelay(new ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceClient(channel)),
            "page-composition" => new CopyGoalWorkflowIdPageCompositionRelay(new ApiWebbff.PageCompositionService.PageCompositionServiceClient(channel)),
            "device-policy-lifecycle" => new CopyGoalWorkflowIdDevicePolicyLifecycleTarget(new ApiDevices.DevicePolicyLifecycleService.DevicePolicyLifecycleServiceClient(channel)),
            _ => new CopyGoalWorkflowIdLogOnly(),
        };
        await sink.DeliverAsync(record, token);
    }
}
