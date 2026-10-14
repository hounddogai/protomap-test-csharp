using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiConcessions = Acme.Concessions.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiPersonalization = Acme.Personalization.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Ops.ContractorsAuditor;

/// <summary>Where ForwardOnboardingTaskLifecycleId hands each record.</summary>
internal interface IForwardOnboardingTaskLifecycleIdTarget
{
    Task PushAsync(ForwardOnboardingTaskLifecycleIdRecord record, CancellationToken token);
}

/// <summary>What ForwardOnboardingTaskLifecycleId keeps of each GetOnboardingTaskLifecycle response.</summary>
internal sealed class ForwardOnboardingTaskLifecycleIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Subject { get; init; } = "";
}

/// <summary>Sends records to acme.recruiting.v1.CandidateApprovalService.</summary>
internal sealed class ForwardOnboardingTaskLifecycleIdCandidateApprovalWriter(ApiRecruiting.CandidateApprovalService.CandidateApprovalServiceClient client) : IForwardOnboardingTaskLifecycleIdTarget
{
    public async Task PushAsync(ForwardOnboardingTaskLifecycleIdRecord record, CancellationToken token)
    {
        await client.RejectCandidateAsync(new ApiRecruiting.RejectCandidateRequest { Reason = record.Subject, Details = new ApiRecruiting.RejectCandidateRequest.Types.Details { ApproverEmail = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.concessions.v1.ConcessionStandService.</summary>
internal sealed class ForwardOnboardingTaskLifecycleIdConcessionStandRelay(ApiConcessions.ConcessionStandService.ConcessionStandServiceClient client) : IForwardOnboardingTaskLifecycleIdTarget
{
    public async Task PushAsync(ForwardOnboardingTaskLifecycleIdRecord record, CancellationToken token)
    {
        await client.GetConcessionStandAsync(new ApiConcessions.GetConcessionStandRequest { ConcessionStandId = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.personalization.v1.ProfileService.</summary>
internal sealed class ForwardOnboardingTaskLifecycleIdProfileSink(ApiPersonalization.ProfileService.ProfileServiceClient client) : IForwardOnboardingTaskLifecycleIdTarget
{
    public async Task PushAsync(ForwardOnboardingTaskLifecycleIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiPersonalization.SearchProfilesRequest { Filter = new ApiPersonalization.SearchProfilesRequest.Types.Filter { Region = record.CorrelationId } }, token, client.SearchProfilesAsync);
    }
}

/// <summary>Reads acme.onboarding.v1.OnboardingTaskLifecycleService/GetOnboardingTaskLifecycle and hands what it keeps to the sink of its kind.</summary>
internal static class ForwardOnboardingTaskLifecycleId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiOnboarding.OnboardingTaskLifecycleService.OnboardingTaskLifecycleServiceClient(channel);
        var found = await source.GetOnboardingTaskLifecycleAsync(new ApiOnboarding.GetOnboardingTaskLifecycleRequest() { OnboardingTaskId = "current-forward-onboarding-task-lifecycle-id" }, cancellationToken: token);
        var record = new ForwardOnboardingTaskLifecycleIdRecord
        {
            CorrelationId = Text.Normalize(found.Result.CorrelationId),
            Subject = found.OnboardingTask.Subject,
        };
        await ActOnForwardOnboardingTaskLifecycleIdAsync("candidate-approval", record, channel, token);
    }

    private static async Task ActOnForwardOnboardingTaskLifecycleIdAsync(string kind, ForwardOnboardingTaskLifecycleIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["candidate-approval"] = () => new ForwardOnboardingTaskLifecycleIdCandidateApprovalWriter(new ApiRecruiting.CandidateApprovalService.CandidateApprovalServiceClient(channel)).PushAsync(record, token),
            ["concession-stand"] = () => new ForwardOnboardingTaskLifecycleIdConcessionStandRelay(new ApiConcessions.ConcessionStandService.ConcessionStandServiceClient(channel)).PushAsync(record, token),
            ["profile"] = () => new ForwardOnboardingTaskLifecycleIdProfileSink(new ApiPersonalization.ProfileService.ProfileServiceClient(channel)).PushAsync(record, token),
        };
        await actions[kind]();
    }
}
