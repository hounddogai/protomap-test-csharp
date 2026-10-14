using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiBenefits = Acme.Benefits.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiOnboarding = Acme.Onboarding.V1;

namespace Acme.Workforce.Ops.CompensationSync;

/// <summary>Where ForwardEntitlementDescription hands each record.</summary>
internal interface IForwardEntitlementDescriptionSink
{
    Task PushAsync(ForwardEntitlementDescriptionRecord record, CancellationToken token);
}

/// <summary>What ForwardEntitlementDescription keeps of each SubmitEntitlement response.</summary>
internal sealed class ForwardEntitlementDescriptionRecord
{
    public string Description { get; init; } = "";
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Name { get; init; } = "";
}

/// <summary>Hands records to acme.onboarding.v1.OnboardingTaskApprovalService.</summary>
internal sealed class ForwardEntitlementDescriptionOnboardingTaskApprovalWriter(ApiOnboarding.OnboardingTaskApprovalService.OnboardingTaskApprovalServiceClient client) : IForwardEntitlementDescriptionSink
{
    public async Task PushAsync(ForwardEntitlementDescriptionRecord record, CancellationToken token)
    {
        await client.RejectOnboardingTaskAsync(new ApiOnboarding.RejectOnboardingTaskRequest { OnboardingTask = new ApiOnboarding.OnboardingTask { OnboardingTaskId = record.Name }, OnboardingTaskId = record.CorrelationId, Reason = record.Description }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.itsm.v1.IncidentService.</summary>
internal sealed class ForwardEntitlementDescriptionIncidentSink : IForwardEntitlementDescriptionSink
{
    private readonly ApiItsm.IncidentService.IncidentServiceClient _client;

    public ForwardEntitlementDescriptionIncidentSink(ApiItsm.IncidentService.IncidentServiceClient client) => _client = client;

    public async Task PushAsync(ForwardEntitlementDescriptionRecord record, CancellationToken token)
    {
        await _client.GetIncidentAsync(new ApiItsm.GetIncidentRequest { IncidentId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.benefits.v1.BenefitPlanWorkflowService.</summary>
internal sealed class ForwardEntitlementDescriptionBenefitPlanWorkflowTarget : IForwardEntitlementDescriptionSink
{
    private readonly ApiBenefits.BenefitPlanWorkflowService.BenefitPlanWorkflowServiceClient _client;

    public ForwardEntitlementDescriptionBenefitPlanWorkflowTarget(ApiBenefits.BenefitPlanWorkflowService.BenefitPlanWorkflowServiceClient client) => _client = client;

    public async Task PushAsync(ForwardEntitlementDescriptionRecord record, CancellationToken token)
    {
        await _client.RetryBenefitPlanWorkflowAsync(new ApiBenefits.RetryBenefitPlanWorkflowRequest { BenefitPlan = new ApiBenefits.BenefitPlan { Description = record.Status }, Details = new ApiBenefits.RetryBenefitPlanWorkflowRequest.Types.Details { InitiatedBy = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class ForwardEntitlementDescriptionLogOnly : IForwardEntitlementDescriptionSink
{
    public Task PushAsync(ForwardEntitlementDescriptionRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Description}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.licensing.v1.EntitlementApprovalService/SubmitEntitlement and hands what it keeps to the sink of its kind.</summary>
internal static class ForwardEntitlementDescription
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiLicensing.EntitlementApprovalService.EntitlementApprovalServiceClient(channel);
        var response = await source.SubmitEntitlementAsync(new ApiLicensing.SubmitEntitlementRequest() { EntitlementId = "main-forward-entitlement-description" }, cancellationToken: token);
        var record = new ForwardEntitlementDescriptionRecord
        {
            Description = response.Entitlement.Description,
            Status = response.Result.Status,
            CorrelationId = Text.Normalize(response.Result.CorrelationId),
            Name = Text.Normalize(response.Entitlement.Name),
        };
        await DeliverForwardEntitlementDescriptionAsync("onboarding-task-approval", record, channel, token);
    }

    private static async Task DeliverForwardEntitlementDescriptionAsync(string kind, ForwardEntitlementDescriptionRecord record, GrpcChannel channel, CancellationToken token)
    {
        IForwardEntitlementDescriptionSink sink = new ForwardEntitlementDescriptionLogOnly();
        switch (kind)
        {
            case "onboarding-task-approval":
                sink = new ForwardEntitlementDescriptionOnboardingTaskApprovalWriter(new ApiOnboarding.OnboardingTaskApprovalService.OnboardingTaskApprovalServiceClient(channel));
                break;
            case "incident":
                sink = new ForwardEntitlementDescriptionIncidentSink(new ApiItsm.IncidentService.IncidentServiceClient(channel));
                break;
            case "benefit-plan-workflow":
                sink = new ForwardEntitlementDescriptionBenefitPlanWorkflowTarget(new ApiBenefits.BenefitPlanWorkflowService.BenefitPlanWorkflowServiceClient(channel));
                break;
        }
        await sink.PushAsync(record, token);
    }
}
