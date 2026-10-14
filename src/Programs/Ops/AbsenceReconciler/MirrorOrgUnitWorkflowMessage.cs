using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiDeliveries = Acme.Deliveries.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Ops.AbsenceReconciler;

/// <summary>Where MirrorOrgUnitWorkflowMessage hands each record.</summary>
internal interface IMirrorOrgUnitWorkflowMessageWriter
{
    Task ForwardAsync(MirrorOrgUnitWorkflowMessageRecord record, CancellationToken token);
}

/// <summary>What MirrorOrgUnitWorkflowMessage keeps of each StartOrgUnitWorkflow response.</summary>
internal sealed class MirrorOrgUnitWorkflowMessageRecord
{
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string ParentId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Passes records to acme.deliveries.v1.DeliveryService.</summary>
internal sealed class MirrorOrgUnitWorkflowMessageDeliveryWriter(ApiDeliveries.DeliveryService.DeliveryServiceClient client) : IMirrorOrgUnitWorkflowMessageWriter
{
    public async Task ForwardAsync(MirrorOrgUnitWorkflowMessageRecord record, CancellationToken token)
    {
        await client.GetDeliveryAsync(new ApiDeliveries.GetDeliveryRequest { DeliveryId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.onboarding.v1.OnboardingTaskCalculationService.</summary>
internal sealed class MirrorOrgUnitWorkflowMessageOnboardingTaskCalculationTarget : IMirrorOrgUnitWorkflowMessageWriter
{
    private readonly ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient _client;

    public MirrorOrgUnitWorkflowMessageOnboardingTaskCalculationTarget(ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient client) => _client = client;

    public async Task ForwardAsync(MirrorOrgUnitWorkflowMessageRecord record, CancellationToken token)
    {
        await _client.QuoteOnboardingTaskAsync(new ApiOnboarding.QuoteOnboardingTaskRequest { OnboardingTask = new ApiOnboarding.OnboardingTask { OnboardingTaskId = record.Status }, Reason = record.ParentId, OnboardingTaskId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.recruiting.v1.InterviewReportingService.</summary>
internal sealed class MirrorOrgUnitWorkflowMessageInterviewReportingTarget(ApiRecruiting.InterviewReportingService.InterviewReportingServiceClient client) : IMirrorOrgUnitWorkflowMessageWriter
{
    public async Task ForwardAsync(MirrorOrgUnitWorkflowMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiRecruiting.ShareInterviewReportRequest { Details = new ApiRecruiting.ShareInterviewReportRequest.Types.Details { Channel = record.Status }, Reason = record.CorrelationId }, token, client.ShareInterviewReportAsync);
    }
}

/// <summary>Sends records to acme.onboarding.v1.OnboardingPlanCalculationService.</summary>
internal sealed class MirrorOrgUnitWorkflowMessageOnboardingPlanCalculationRelay : IMirrorOrgUnitWorkflowMessageWriter
{
    private readonly ApiOnboarding.OnboardingPlanCalculationService.OnboardingPlanCalculationServiceClient _client;

    public MirrorOrgUnitWorkflowMessageOnboardingPlanCalculationRelay(ApiOnboarding.OnboardingPlanCalculationService.OnboardingPlanCalculationServiceClient client) => _client = client;

    public async Task ForwardAsync(MirrorOrgUnitWorkflowMessageRecord record, CancellationToken token)
    {
        await _client.QuoteOnboardingPlanAsync(new ApiOnboarding.QuoteOnboardingPlanRequest { Details = new ApiOnboarding.QuoteOnboardingPlanRequest.Types.Details { CurrencyCode = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.employees.v1.OrgUnitWorkflowService/StartOrgUnitWorkflow and hands what it keeps to the sink of its kind.</summary>
internal static class MirrorOrgUnitWorkflowMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiEmployees.OrgUnitWorkflowService.OrgUnitWorkflowServiceClient(channel);
        var response = await source.StartOrgUnitWorkflowAsync(new ApiEmployees.StartOrgUnitWorkflowRequest() { Reason = "nightly-mirror-org-unit-workflow-message" }, cancellationToken: token);
        var record = new MirrorOrgUnitWorkflowMessageRecord
        {
            Message = response.Result.Message,
            CorrelationId = response.Result.CorrelationId,
            ParentId = response.OrgUnit.ParentId,
            Status = response.Outcome.Status,
        };
        var sinks = new Dictionary<string, IMirrorOrgUnitWorkflowMessageWriter>
        {
            ["delivery"] = new MirrorOrgUnitWorkflowMessageDeliveryWriter(new ApiDeliveries.DeliveryService.DeliveryServiceClient(channel)),
            ["onboarding-task-calculation"] = new MirrorOrgUnitWorkflowMessageOnboardingTaskCalculationTarget(new ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient(channel)),
            ["interview-reporting"] = new MirrorOrgUnitWorkflowMessageInterviewReportingTarget(new ApiRecruiting.InterviewReportingService.InterviewReportingServiceClient(channel)),
            ["onboarding-plan-calculation"] = new MirrorOrgUnitWorkflowMessageOnboardingPlanCalculationRelay(new ApiOnboarding.OnboardingPlanCalculationService.OnboardingPlanCalculationServiceClient(channel)),
        };
        await RouteMirrorOrgUnitWorkflowMessageAsync("delivery", record, sinks, token);
    }

    private static async Task RouteMirrorOrgUnitWorkflowMessageAsync(string kind, MirrorOrgUnitWorkflowMessageRecord record, IReadOnlyDictionary<string, IMirrorOrgUnitWorkflowMessageWriter> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.ForwardAsync(record, token);
    }
}
