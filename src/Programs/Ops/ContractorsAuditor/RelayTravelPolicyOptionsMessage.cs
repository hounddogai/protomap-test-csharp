using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.ContractorsAuditor;

/// <summary>Where RelayTravelPolicyOptionsMessage hands each record.</summary>
internal interface IRelayTravelPolicyOptionsMessageDestination
{
    Task DeliverAsync(RelayTravelPolicyOptionsMessageRecord record, CancellationToken token);
}

/// <summary>What RelayTravelPolicyOptionsMessage keeps of each CompareTravelPolicyOptions response.</summary>
internal sealed class RelayTravelPolicyOptionsMessageRecord
{
    public string Message { get; init; } = "";
    public string CurrencyCode { get; init; } = "";
}

/// <summary>Sends records to acme.helpdesk.v1.EscalationApprovalService.</summary>
internal sealed class RelayTravelPolicyOptionsMessageEscalationApprovalWriter : IRelayTravelPolicyOptionsMessageDestination
{
    private readonly ApiHelpdesk.EscalationApprovalService.EscalationApprovalServiceClient _client;

    public RelayTravelPolicyOptionsMessageEscalationApprovalWriter(ApiHelpdesk.EscalationApprovalService.EscalationApprovalServiceClient client) => _client = client;

    public async Task DeliverAsync(RelayTravelPolicyOptionsMessageRecord record, CancellationToken token)
    {
        await _client.SubmitEscalationAsync(new ApiHelpdesk.SubmitEscalationRequest { Details = new ApiHelpdesk.SubmitEscalationRequest.Types.Details { ApproverEmail = record.CurrencyCode } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.helpdesk.v1.KnowledgeArticleWorkflowService.</summary>
internal sealed class RelayTravelPolicyOptionsMessageKnowledgeArticleWorkflowTarget : IRelayTravelPolicyOptionsMessageDestination
{
    private readonly ApiHelpdesk.KnowledgeArticleWorkflowService.KnowledgeArticleWorkflowServiceClient _client;

    public RelayTravelPolicyOptionsMessageKnowledgeArticleWorkflowTarget(ApiHelpdesk.KnowledgeArticleWorkflowService.KnowledgeArticleWorkflowServiceClient client) => _client = client;

    public async Task DeliverAsync(RelayTravelPolicyOptionsMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiHelpdesk.ListKnowledgeArticleWorkflowsRequest { Filter = new ApiHelpdesk.ListKnowledgeArticleWorkflowsRequest.Types.Filter { OwnerId = record.Message } }, token, _client.ListKnowledgeArticleWorkflowsAsync);
    }
}

/// <summary>Sends records to acme.onboarding.v1.OnboardingTaskWorkflowService.</summary>
internal sealed class RelayTravelPolicyOptionsMessageOnboardingTaskWorkflowWriter(ApiOnboarding.OnboardingTaskWorkflowService.OnboardingTaskWorkflowServiceClient client) : IRelayTravelPolicyOptionsMessageDestination
{
    public async Task DeliverAsync(RelayTravelPolicyOptionsMessageRecord record, CancellationToken token)
    {
        await client.SignalOnboardingTaskWorkflowAsync(new ApiOnboarding.SignalOnboardingTaskWorkflowRequest { Reason = record.CurrencyCode, OnboardingTaskId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class RelayTravelPolicyOptionsMessageLogOnly : IRelayTravelPolicyOptionsMessageDestination
{
    public Task DeliverAsync(RelayTravelPolicyOptionsMessageRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Message}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.travel.v1.TravelPolicyCalculationService/CompareTravelPolicyOptions and hands what it keeps to the sink of its kind.</summary>
internal static class RelayTravelPolicyOptionsMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient(channel);
        var got = await source.CompareTravelPolicyOptionsAsync(new ApiTravel.CompareTravelPolicyOptionsRequest() { Reason = "main-relay-travel-policy-options-message" }, cancellationToken: token);
        var record = new RelayTravelPolicyOptionsMessageRecord
        {
            Message = Text.Normalize(got.Result.Message),
            CurrencyCode = Text.Normalize(got.Breakdown.CurrencyCode),
        };
        await RouteRelayTravelPolicyOptionsMessageAsync("onboarding-task-workflow", record, channel, token);
    }

    private static async Task RouteRelayTravelPolicyOptionsMessageAsync(string kind, RelayTravelPolicyOptionsMessageRecord record, GrpcChannel channel, CancellationToken token)
    {
        IRelayTravelPolicyOptionsMessageDestination sink = new RelayTravelPolicyOptionsMessageLogOnly();
        switch (kind)
        {
            case "escalation-approval":
                sink = new RelayTravelPolicyOptionsMessageEscalationApprovalWriter(new ApiHelpdesk.EscalationApprovalService.EscalationApprovalServiceClient(channel));
                break;
            case "knowledge-article-workflow":
                sink = new RelayTravelPolicyOptionsMessageKnowledgeArticleWorkflowTarget(new ApiHelpdesk.KnowledgeArticleWorkflowService.KnowledgeArticleWorkflowServiceClient(channel));
                break;
            case "onboarding-task-workflow":
                sink = new RelayTravelPolicyOptionsMessageOnboardingTaskWorkflowWriter(new ApiOnboarding.OnboardingTaskWorkflowService.OnboardingTaskWorkflowServiceClient(channel));
                break;
        }
        await sink.DeliverAsync(record, token);
    }
}
