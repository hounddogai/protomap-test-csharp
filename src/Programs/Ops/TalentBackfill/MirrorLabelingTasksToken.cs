using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiDocuments = Acme.Documents.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLabeling = Acme.Labeling.V1;
using ApiOnboarding = Acme.Onboarding.V1;

namespace Acme.Workforce.Ops.TalentBackfill;

/// <summary>Where MirrorLabelingTasksToken hands each record.</summary>
internal interface IMirrorLabelingTasksTokenWriter
{
    Task ForwardAsync(MirrorLabelingTasksTokenRecord record, CancellationToken token);
}

/// <summary>What MirrorLabelingTasksToken keeps of each SuggestLabelingTasks response.</summary>
internal sealed class MirrorLabelingTasksTokenRecord
{
    public string NextPageToken { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Hands records to acme.onboarding.v1.OnboardingPlanService.</summary>
internal sealed class MirrorLabelingTasksTokenOnboardingPlanTarget(ApiOnboarding.OnboardingPlanService.OnboardingPlanServiceClient client) : IMirrorLabelingTasksTokenWriter
{
    public async Task ForwardAsync(MirrorLabelingTasksTokenRecord record, CancellationToken token)
    {
        await client.CreateOnboardingPlanAsync(new ApiOnboarding.CreateOnboardingPlanRequest { OnboardingPlan = new ApiOnboarding.OnboardingPlan { Number = record.NextPageToken, OnboardingPlanId = record.CorrelationId }, RequestId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.itsm.v1.IncidentLifecycleService.</summary>
internal sealed class MirrorLabelingTasksTokenIncidentLifecycleRelay(ApiItsm.IncidentLifecycleService.IncidentLifecycleServiceClient client) : IMirrorLabelingTasksTokenWriter
{
    public async Task ForwardAsync(MirrorLabelingTasksTokenRecord record, CancellationToken token)
    {
        await client.TerminateIncidentAsync(new ApiItsm.TerminateIncidentRequest { Reason = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.documents.v1.DocumentTemplateSyncService.</summary>
internal sealed class MirrorLabelingTasksTokenDocumentTemplateSyncSink(ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient client) : IMirrorLabelingTasksTokenWriter
{
    public async Task ForwardAsync(MirrorLabelingTasksTokenRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiDocuments.ResolveDocumentTemplateConflictRequest { Details = new ApiDocuments.ResolveDocumentTemplateConflictRequest.Types.Details { Cursor = record.NextPageToken, PeerRegion = record.Status, Resolution = record.CorrelationId } }, token, client.ResolveDocumentTemplateConflictAsync);
    }
}

/// <summary>Reads acme.labeling.v1.LabelingTaskQueryService/SuggestLabelingTasks and hands what it keeps to the sink of its kind.</summary>
internal static class MirrorLabelingTasksToken
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiLabeling.LabelingTaskQueryService.LabelingTaskQueryServiceClient(channel);
        var got = await source.SuggestLabelingTasksAsync(new ApiLabeling.SuggestLabelingTasksRequest() { OrderBy = "main-mirror-labeling-tasks-token" }, cancellationToken: token);
        var record = new MirrorLabelingTasksTokenRecord
        {
            NextPageToken = got.PageInfo.NextPageToken,
            CorrelationId = got.Result.CorrelationId,
            Status = got.Result.Status,
        };
        var sinks = new Dictionary<string, IMirrorLabelingTasksTokenWriter>
        {
            ["onboarding-plan"] = new MirrorLabelingTasksTokenOnboardingPlanTarget(new ApiOnboarding.OnboardingPlanService.OnboardingPlanServiceClient(channel)),
            ["incident-lifecycle"] = new MirrorLabelingTasksTokenIncidentLifecycleRelay(new ApiItsm.IncidentLifecycleService.IncidentLifecycleServiceClient(channel)),
            ["document-template-sync"] = new MirrorLabelingTasksTokenDocumentTemplateSyncSink(new ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient(channel)),
        };
        await RouteMirrorLabelingTasksTokenAsync("incident-lifecycle", record, sinks, token);
    }

    private static async Task RouteMirrorLabelingTasksTokenAsync(string kind, MirrorLabelingTasksTokenRecord record, IReadOnlyDictionary<string, IMirrorLabelingTasksTokenWriter> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.ForwardAsync(record, token);
    }
}
