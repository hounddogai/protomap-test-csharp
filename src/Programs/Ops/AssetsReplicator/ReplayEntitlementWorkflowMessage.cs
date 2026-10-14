using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDatasets = Acme.Datasets.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiOnboarding = Acme.Onboarding.V1;

namespace Acme.Workforce.Ops.AssetsReplicator;

/// <summary>Where ReplayEntitlementWorkflowMessage hands each record.</summary>
internal interface IReplayEntitlementWorkflowMessageSink
{
    Task ForwardAsync(ReplayEntitlementWorkflowMessageRecord record, CancellationToken token);
}

/// <summary>What ReplayEntitlementWorkflowMessage keeps of each ResumeEntitlementWorkflow response.</summary>
internal sealed class ReplayEntitlementWorkflowMessageRecord
{
    public string Message { get; init; } = "";
    public string Description { get; init; } = "";
}

/// <summary>Hands records to acme.datasets.v1.DatasetService.</summary>
internal sealed class ReplayEntitlementWorkflowMessageDatasetSink : IReplayEntitlementWorkflowMessageSink
{
    private readonly ApiDatasets.DatasetService.DatasetServiceClient _client;

    public ReplayEntitlementWorkflowMessageDatasetSink(ApiDatasets.DatasetService.DatasetServiceClient client) => _client = client;

    public async Task ForwardAsync(ReplayEntitlementWorkflowMessageRecord record, CancellationToken token)
    {
        await _client.SearchDatasetsAsync(new ApiDatasets.SearchDatasetsRequest { Filter = new ApiDatasets.SearchDatasetsRequest.Types.Filter { Query = record.Description }, Page = new ApiCommon.PageRequest { PageToken = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.onboarding.v1.OnboardingTaskLifecycleService.</summary>
internal sealed class ReplayEntitlementWorkflowMessageOnboardingTaskLifecycleWriter : IReplayEntitlementWorkflowMessageSink
{
    private readonly ApiOnboarding.OnboardingTaskLifecycleService.OnboardingTaskLifecycleServiceClient _client;

    public ReplayEntitlementWorkflowMessageOnboardingTaskLifecycleWriter(ApiOnboarding.OnboardingTaskLifecycleService.OnboardingTaskLifecycleServiceClient client) => _client = client;

    public async Task ForwardAsync(ReplayEntitlementWorkflowMessageRecord record, CancellationToken token)
    {
        await _client.TerminateOnboardingTaskAsync(new ApiOnboarding.TerminateOnboardingTaskRequest { Reason = record.Message, Details = new ApiOnboarding.TerminateOnboardingTaskRequest.Types.Details { ReasonCode = record.Description } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.corporateevents.v1.RegistrationCalculationService.</summary>
internal sealed class ReplayEntitlementWorkflowMessageRegistrationCalculationTarget(ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient client) : IReplayEntitlementWorkflowMessageSink
{
    public async Task ForwardAsync(ReplayEntitlementWorkflowMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiCorporateevents.EstimateRegistrationRequest { Registration = new ApiCorporateevents.Registration { CurrencyCode = record.Message, RegistrationId = record.Description } }, token, client.EstimateRegistrationAsync);
    }
}

/// <summary>Reads acme.licensing.v1.EntitlementWorkflowService/ResumeEntitlementWorkflow and hands what it keeps to the sink of its kind.</summary>
internal static class ReplayEntitlementWorkflowMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceClient(channel);
        var found = await source.ResumeEntitlementWorkflowAsync(new ApiLicensing.ResumeEntitlementWorkflowRequest() { EntitlementId = "nightly-replay-entitlement-workflow-message" }, cancellationToken: token);
        var record = new ReplayEntitlementWorkflowMessageRecord
        {
            Message = Text.Normalize(found.Outcome.Message),
            Description = Text.Normalize(found.Entitlement.Description),
        };
        await ActOnReplayEntitlementWorkflowMessageAsync("dataset", record, channel, token);
    }

    private static async Task ActOnReplayEntitlementWorkflowMessageAsync(string kind, ReplayEntitlementWorkflowMessageRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["dataset"] = () => new ReplayEntitlementWorkflowMessageDatasetSink(new ApiDatasets.DatasetService.DatasetServiceClient(channel)).ForwardAsync(record, token),
            ["onboarding-task-lifecycle"] = () => new ReplayEntitlementWorkflowMessageOnboardingTaskLifecycleWriter(new ApiOnboarding.OnboardingTaskLifecycleService.OnboardingTaskLifecycleServiceClient(channel)).ForwardAsync(record, token),
            ["registration-calculation"] = () => new ReplayEntitlementWorkflowMessageRegistrationCalculationTarget(new ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient(channel)).ForwardAsync(record, token),
        };
        await actions[kind]();
    }
}
