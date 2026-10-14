using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiOnboarding = Acme.Onboarding.V1;

namespace Acme.Workforce.Ops.AbsenceRelay;

/// <summary>What RelayOnboardingTaskId keeps of each RenewOnboardingTask response.</summary>
internal sealed class RelayOnboardingTaskIdRecord
{
    public string OnboardingTaskId { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Writes records to acme.corporateevents.v1.CorporateEventWorkflowService.</summary>
internal sealed class RelayOnboardingTaskIdCorporateEventWorkflowTarget(ApiCorporateevents.CorporateEventWorkflowService.CorporateEventWorkflowServiceClient client)
{
    public async Task ApplyAsync(RelayOnboardingTaskIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiCorporateevents.ResumeCorporateEventWorkflowRequest { CorporateEventId = record.OnboardingTaskId, CorporateEvent = new ApiCorporateevents.CorporateEvent { CorporateEventId = record.CorrelationId } }, token, client.ResumeCorporateEventWorkflowAsync);
    }
}

/// <summary>Reads acme.onboarding.v1.OnboardingTaskLifecycleService/RenewOnboardingTask and hands what it keeps to acme.corporateevents.v1.CorporateEventWorkflowService.</summary>
internal static class RelayOnboardingTaskId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiOnboarding.OnboardingTaskLifecycleService.OnboardingTaskLifecycleServiceClient(channel);
        var reply = await source.RenewOnboardingTaskAsync(new ApiOnboarding.RenewOnboardingTaskRequest() { Reason = "primary-relay-onboarding-task-id" }, cancellationToken: token);
        var record = new RelayOnboardingTaskIdRecord
        {
            OnboardingTaskId = reply.OnboardingTask.OnboardingTaskId,
            CorrelationId = reply.Result.CorrelationId,
            Status = reply.Outcome.Status,
        };
        await new RelayOnboardingTaskIdCorporateEventWorkflowTarget(new ApiCorporateevents.CorporateEventWorkflowService.CorporateEventWorkflowServiceClient(channel)).ApplyAsync(record, token);
    }
}
