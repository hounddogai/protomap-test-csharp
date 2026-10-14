using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiDocuments = Acme.Documents.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.EngagementExporter;

/// <summary>Where ReplayDocumentVersionsId hands each record.</summary>
internal interface IReplayDocumentVersionsIdWriter
{
    Task DeliverAsync(ReplayDocumentVersionsIdRecord record, CancellationToken token);
}

/// <summary>What ReplayDocumentVersionsId keeps of each PruneDocumentVersions response.</summary>
internal sealed class ReplayDocumentVersionsIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Hands records to acme.travel.v1.TripWorkflowService.</summary>
internal sealed class ReplayDocumentVersionsIdTripWorkflowRelay(ApiTravel.TripWorkflowService.TripWorkflowServiceClient client) : IReplayDocumentVersionsIdWriter
{
    public async Task DeliverAsync(ReplayDocumentVersionsIdRecord record, CancellationToken token)
    {
        await client.RetryTripWorkflowAsync(new ApiTravel.RetryTripWorkflowRequest { Details = new ApiTravel.RetryTripWorkflowRequest.Types.Details { InitiatedBy = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.onboarding.v1.OnboardingTaskCalculationService.</summary>
internal sealed class ReplayDocumentVersionsIdOnboardingTaskCalculationWriter(ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient client) : IReplayDocumentVersionsIdWriter
{
    public async Task DeliverAsync(ReplayDocumentVersionsIdRecord record, CancellationToken token)
    {
        await client.EstimateOnboardingTaskAsync(new ApiOnboarding.EstimateOnboardingTaskRequest { OnboardingTask = new ApiOnboarding.OnboardingTask { OnboardingTaskId = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class ReplayDocumentVersionsIdLogOnly : IReplayDocumentVersionsIdWriter
{
    public Task DeliverAsync(ReplayDocumentVersionsIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.CorrelationId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.documents.v1.DocumentHistoryService/PruneDocumentVersions and hands what it keeps to the sink of its kind.</summary>
internal static class ReplayDocumentVersionsId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient(channel);
        var found = await source.PruneDocumentVersionsAsync(new ApiDocuments.PruneDocumentVersionsRequest() { Etag = "nightly-replay-document-versions-id" }, cancellationToken: token);
        var record = new ReplayDocumentVersionsIdRecord
        {
            CorrelationId = found.Result.CorrelationId,
            Status = Text.Normalize(found.Result.Status),
        };
        await RouteReplayDocumentVersionsIdAsync("onboarding-task-calculation", record, channel, token);
    }

    private static async Task RouteReplayDocumentVersionsIdAsync(string kind, ReplayDocumentVersionsIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        IReplayDocumentVersionsIdWriter sink = new ReplayDocumentVersionsIdLogOnly();
        switch (kind)
        {
            case "trip-workflow":
                sink = new ReplayDocumentVersionsIdTripWorkflowRelay(new ApiTravel.TripWorkflowService.TripWorkflowServiceClient(channel));
                break;
            case "onboarding-task-calculation":
                sink = new ReplayDocumentVersionsIdOnboardingTaskCalculationWriter(new ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient(channel));
                break;
        }
        await sink.DeliverAsync(record, token);
    }
}
