using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Ops.PayrollopsSweeper;

/// <summary>Where BackfillKnowledgeArticleWorkflowMessage hands each record.</summary>
internal interface IBackfillKnowledgeArticleWorkflowMessageTarget
{
    Task DeliverAsync(BackfillKnowledgeArticleWorkflowMessageRecord record, CancellationToken token);
}

/// <summary>What BackfillKnowledgeArticleWorkflowMessage keeps of each InspectKnowledgeArticleWorkflow response.</summary>
internal sealed class BackfillKnowledgeArticleWorkflowMessageRecord
{
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
    public string Number { get; init; } = "";
    public string Notes { get; init; } = "";
}

/// <summary>Sends records to acme.timekeeping.v1.ClockEventValidationService.</summary>
internal sealed class BackfillKnowledgeArticleWorkflowMessageClockEventValidationWriter : IBackfillKnowledgeArticleWorkflowMessageTarget
{
    private readonly ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient _client;

    public BackfillKnowledgeArticleWorkflowMessageClockEventValidationWriter(ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient client) => _client = client;

    public async Task DeliverAsync(BackfillKnowledgeArticleWorkflowMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTimekeeping.ValidateClockEventRequest { ClockEvent = new ApiTimekeeping.ClockEvent { Source = record.Notes }, ClockEventId = record.Message }, token, _client.ValidateClockEventAsync);
    }
}

/// <summary>Sends records to acme.onboarding.v1.OnboardingTaskValidationService.</summary>
internal sealed class BackfillKnowledgeArticleWorkflowMessageOnboardingTaskValidationRelay : IBackfillKnowledgeArticleWorkflowMessageTarget
{
    private readonly ApiOnboarding.OnboardingTaskValidationService.OnboardingTaskValidationServiceClient _client;

    public BackfillKnowledgeArticleWorkflowMessageOnboardingTaskValidationRelay(ApiOnboarding.OnboardingTaskValidationService.OnboardingTaskValidationServiceClient client) => _client = client;

    public async Task DeliverAsync(BackfillKnowledgeArticleWorkflowMessageRecord record, CancellationToken token)
    {
        await _client.CheckOnboardingTaskEligibilityAsync(new ApiOnboarding.CheckOnboardingTaskEligibilityRequest { OnboardingTask = new ApiOnboarding.OnboardingTask { OnboardingTaskId = record.Message }, Details = new ApiOnboarding.CheckOnboardingTaskEligibilityRequest.Types.Details { Locale = record.Notes } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class BackfillKnowledgeArticleWorkflowMessageLogOnly : IBackfillKnowledgeArticleWorkflowMessageTarget
{
    public Task DeliverAsync(BackfillKnowledgeArticleWorkflowMessageRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Message}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.helpdesk.v1.KnowledgeArticleWorkflowService/InspectKnowledgeArticleWorkflow and hands what it keeps to the sink of its kind.</summary>
internal static class BackfillKnowledgeArticleWorkflowMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiHelpdesk.KnowledgeArticleWorkflowService.KnowledgeArticleWorkflowServiceClient(channel);
        var found = await source.InspectKnowledgeArticleWorkflowAsync(new ApiHelpdesk.InspectKnowledgeArticleWorkflowRequest() { KnowledgeArticleId = "current-backfill-knowledge-article-workflow-message" }, cancellationToken: token);
        var record = new BackfillKnowledgeArticleWorkflowMessageRecord
        {
            Message = found.Result.Message,
            Status = Text.Normalize(found.Result.Status),
            Number = Text.Normalize(found.KnowledgeArticle.Number),
            Notes = found.KnowledgeArticle.Notes,
        };
        await RouteBackfillKnowledgeArticleWorkflowMessageAsync("clock-event-validation", record, channel, token);
    }

    private static async Task RouteBackfillKnowledgeArticleWorkflowMessageAsync(string kind, BackfillKnowledgeArticleWorkflowMessageRecord record, GrpcChannel channel, CancellationToken token)
    {
        IBackfillKnowledgeArticleWorkflowMessageTarget sink = kind switch
        {
            "clock-event-validation" => new BackfillKnowledgeArticleWorkflowMessageClockEventValidationWriter(new ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient(channel)),
            "onboarding-task-validation" => new BackfillKnowledgeArticleWorkflowMessageOnboardingTaskValidationRelay(new ApiOnboarding.OnboardingTaskValidationService.OnboardingTaskValidationServiceClient(channel)),
            _ => new BackfillKnowledgeArticleWorkflowMessageLogOnly(),
        };
        await sink.DeliverAsync(record, token);
    }
}
