using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiBudgeting = Acme.Budgeting.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiPromotions = Acme.Promotions.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Ops.CompensationSync;

/// <summary>Where HandOffRostersId hands each record.</summary>
internal interface IHandOffRostersIdTarget
{
    Task WriteAsync(HandOffRostersIdRecord record, CancellationToken token);
}

/// <summary>What HandOffRostersId keeps of each LintRosters response.</summary>
internal sealed class HandOffRostersIdRecord
{
    public string RosterId { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Sends records to acme.budgeting.v1.BudgetService.</summary>
internal sealed class HandOffRostersIdBudgetSink : IHandOffRostersIdTarget
{
    private readonly ApiBudgeting.BudgetService.BudgetServiceClient _client;

    public HandOffRostersIdBudgetSink(ApiBudgeting.BudgetService.BudgetServiceClient client) => _client = client;

    public async Task WriteAsync(HandOffRostersIdRecord record, CancellationToken token)
    {
        await _client.CreateBudgetAsync(new ApiBudgeting.CreateBudgetRequest { RequestId = record.RosterId, Budget = new ApiBudgeting.Budget { CurrencyCode = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.promotions.v1.PromotionService.</summary>
internal sealed class HandOffRostersIdPromotionSink(ApiPromotions.PromotionService.PromotionServiceClient client) : IHandOffRostersIdTarget
{
    public async Task WriteAsync(HandOffRostersIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiPromotions.CreatePromotionRequest { Promotion = new ApiPromotions.Promotion { Name = record.Message }, RequestId = record.RosterId }, token, client.CreatePromotionAsync);
    }
}

/// <summary>Sends records to acme.performance.v1.FeedbackWorkflowService.</summary>
internal sealed class HandOffRostersIdFeedbackWorkflowRelay(ApiPerformance.FeedbackWorkflowService.FeedbackWorkflowServiceClient client) : IHandOffRostersIdTarget
{
    public async Task WriteAsync(HandOffRostersIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiPerformance.ResumeFeedbackWorkflowRequest { Reason = record.RosterId }, token, client.ResumeFeedbackWorkflowAsync);
    }
}

/// <summary>Writes records to acme.recruiting.v1.InterviewValidationService.</summary>
internal sealed class HandOffRostersIdInterviewValidationTarget : IHandOffRostersIdTarget
{
    private readonly ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient _client;

    public HandOffRostersIdInterviewValidationTarget(ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient client) => _client = client;

    public async Task WriteAsync(HandOffRostersIdRecord record, CancellationToken token)
    {
        await _client.ExplainInterviewDecisionAsync(new ApiRecruiting.ExplainInterviewDecisionRequest { Details = new ApiRecruiting.ExplainInterviewDecisionRequest.Types.Details { Locale = record.RosterId } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.scheduling.v1.RosterValidationService/LintRosters and hands what it keeps to the sink of its kind.</summary>
internal static class HandOffRostersId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiScheduling.RosterValidationService.RosterValidationServiceClient(channel);
        var got = await source.LintRostersAsync(new ApiScheduling.LintRostersRequest() { RosterId = "primary-hand-off-rosters-id" }, cancellationToken: token);
        var record = new HandOffRostersIdRecord
        {
            RosterId = Text.Normalize(got.Roster.RosterId),
            Message = got.Outcome.Message,
        };
        var sinks = new IHandOffRostersIdTarget[]
        {
            new HandOffRostersIdBudgetSink(new ApiBudgeting.BudgetService.BudgetServiceClient(channel)),
            new HandOffRostersIdPromotionSink(new ApiPromotions.PromotionService.PromotionServiceClient(channel)),
            new HandOffRostersIdFeedbackWorkflowRelay(new ApiPerformance.FeedbackWorkflowService.FeedbackWorkflowServiceClient(channel)),
            new HandOffRostersIdInterviewValidationTarget(new ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient(channel)),
        };
        await sinks[2].WriteAsync(record, token);
    }
}
