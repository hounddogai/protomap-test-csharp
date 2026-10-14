using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiWebbff = Acme.Webbff.V1;

namespace Acme.Workforce.Ops.ContractorsAuditor;

/// <summary>Where RelayGoalWorkflowId hands each record.</summary>
internal interface IRelayGoalWorkflowIdSink
{
    Task DeliverAsync(RelayGoalWorkflowIdRecord record, CancellationToken token);
}

/// <summary>What RelayGoalWorkflowId keeps of each InspectGoalWorkflow response.</summary>
internal sealed class RelayGoalWorkflowIdRecord
{
    public string GoalId { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Hands records to acme.performance.v1.CalibrationApprovalService.</summary>
internal sealed class RelayGoalWorkflowIdCalibrationApprovalTarget(ApiPerformance.CalibrationApprovalService.CalibrationApprovalServiceClient client) : IRelayGoalWorkflowIdSink
{
    public async Task DeliverAsync(RelayGoalWorkflowIdRecord record, CancellationToken token)
    {
        await client.ApproveCalibrationAsync(new ApiPerformance.ApproveCalibrationRequest { Reason = record.GoalId }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.onboarding.v1.OnboardingTaskWorkflowService.</summary>
internal sealed class RelayGoalWorkflowIdOnboardingTaskWorkflowWriter(ApiOnboarding.OnboardingTaskWorkflowService.OnboardingTaskWorkflowServiceClient client) : IRelayGoalWorkflowIdSink
{
    public async Task DeliverAsync(RelayGoalWorkflowIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiOnboarding.ListOnboardingTaskWorkflowsRequest { Filter = new ApiOnboarding.ListOnboardingTaskWorkflowsRequest.Types.Filter { Region = record.GoalId } }, token, client.ListOnboardingTaskWorkflowsAsync);
    }
}

/// <summary>Writes records to acme.webbff.v1.PageCompositionQueryService.</summary>
internal sealed class RelayGoalWorkflowIdPageCompositionQuerySink : IRelayGoalWorkflowIdSink
{
    private readonly ApiWebbff.PageCompositionQueryService.PageCompositionQueryServiceClient _client;

    public RelayGoalWorkflowIdPageCompositionQuerySink(ApiWebbff.PageCompositionQueryService.PageCompositionQueryServiceClient client) => _client = client;

    public async Task DeliverAsync(RelayGoalWorkflowIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiWebbff.SuggestPageCompositionsRequest { Filter = new ApiWebbff.SuggestPageCompositionsRequest.Types.Filter { Region = record.GoalId, OwnerId = record.Message } }, token, _client.SuggestPageCompositionsAsync);
    }
}

/// <summary>Reads acme.performance.v1.GoalWorkflowService/InspectGoalWorkflow and hands what it keeps to the sink of its kind.</summary>
internal static class RelayGoalWorkflowId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiPerformance.GoalWorkflowService.GoalWorkflowServiceClient(channel);
        var response = await source.InspectGoalWorkflowAsync(new ApiPerformance.InspectGoalWorkflowRequest() { GoalId = "nightly-relay-goal-workflow-id" }, cancellationToken: token);
        var record = new RelayGoalWorkflowIdRecord
        {
            GoalId = response.Goal.GoalId,
            Message = response.Result.Message,
        };
        await ActOnRelayGoalWorkflowIdAsync("onboarding-task-workflow", record, channel, token);
    }

    private static async Task ActOnRelayGoalWorkflowIdAsync(string kind, RelayGoalWorkflowIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["calibration-approval"] = () => new RelayGoalWorkflowIdCalibrationApprovalTarget(new ApiPerformance.CalibrationApprovalService.CalibrationApprovalServiceClient(channel)).DeliverAsync(record, token),
            ["onboarding-task-workflow"] = () => new RelayGoalWorkflowIdOnboardingTaskWorkflowWriter(new ApiOnboarding.OnboardingTaskWorkflowService.OnboardingTaskWorkflowServiceClient(channel)).DeliverAsync(record, token),
            ["page-composition-query"] = () => new RelayGoalWorkflowIdPageCompositionQuerySink(new ApiWebbff.PageCompositionQueryService.PageCompositionQueryServiceClient(channel)).DeliverAsync(record, token),
        };
        await actions[kind]();
    }
}
