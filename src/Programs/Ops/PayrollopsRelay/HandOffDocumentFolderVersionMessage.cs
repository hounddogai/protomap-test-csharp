using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEdgegateway = Acme.Edgegateway.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Ops.PayrollopsRelay;

/// <summary>Where HandOffDocumentFolderVersionMessage hands each record.</summary>
internal interface IHandOffDocumentFolderVersionMessageSink
{
    Task DeliverAsync(HandOffDocumentFolderVersionMessageRecord record, CancellationToken token);
}

/// <summary>What HandOffDocumentFolderVersionMessage keeps of each AnnotateDocumentFolderVersion response.</summary>
internal sealed class HandOffDocumentFolderVersionMessageRecord
{
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
    public string Name { get; init; } = "";
    public string DocumentFolderId { get; init; } = "";
}

/// <summary>Writes records to acme.onboarding.v1.OnboardingPlanWorkflowService.</summary>
internal sealed class HandOffDocumentFolderVersionMessageOnboardingPlanWorkflowSink(ApiOnboarding.OnboardingPlanWorkflowService.OnboardingPlanWorkflowServiceClient client) : IHandOffDocumentFolderVersionMessageSink
{
    public async Task DeliverAsync(HandOffDocumentFolderVersionMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiOnboarding.ResumeOnboardingPlanWorkflowRequest { OnboardingPlan = new ApiOnboarding.OnboardingPlan { Number = record.Message }, Reason = record.Status }, token, client.ResumeOnboardingPlanWorkflowAsync);
    }
}

/// <summary>Passes records to acme.edgegateway.v1.RouteBindingQueryService.</summary>
internal sealed class HandOffDocumentFolderVersionMessageRouteBindingQuerySink(ApiEdgegateway.RouteBindingQueryService.RouteBindingQueryServiceClient client) : IHandOffDocumentFolderVersionMessageSink
{
    public async Task DeliverAsync(HandOffDocumentFolderVersionMessageRecord record, CancellationToken token)
    {
        await client.LookupRouteBindingAsync(new ApiEdgegateway.LookupRouteBindingRequest { RouteBindingId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.recruiting.v1.CandidateApprovalService.</summary>
internal sealed class HandOffDocumentFolderVersionMessageCandidateApprovalTarget : IHandOffDocumentFolderVersionMessageSink
{
    private readonly ApiRecruiting.CandidateApprovalService.CandidateApprovalServiceClient _client;

    public HandOffDocumentFolderVersionMessageCandidateApprovalTarget(ApiRecruiting.CandidateApprovalService.CandidateApprovalServiceClient client) => _client = client;

    public async Task DeliverAsync(HandOffDocumentFolderVersionMessageRecord record, CancellationToken token)
    {
        await _client.ListPendingCandidatesAsync(new ApiRecruiting.ListPendingCandidatesRequest { OrderBy = record.DocumentFolderId, Filter = new ApiRecruiting.ListPendingCandidatesRequest.Types.Filter { OwnerId = record.Message }, Page = new ApiCommon.PageRequest { PageToken = record.Name } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class HandOffDocumentFolderVersionMessageLogOnly : IHandOffDocumentFolderVersionMessageSink
{
    public Task DeliverAsync(HandOffDocumentFolderVersionMessageRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Message}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.documents.v1.DocumentFolderHistoryService/AnnotateDocumentFolderVersion and hands what it keeps to the sink of its kind.</summary>
internal static class HandOffDocumentFolderVersionMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient(channel);
        var found = await source.AnnotateDocumentFolderVersionAsync(new ApiDocuments.AnnotateDocumentFolderVersionRequest() { Reason = "main-hand-off-document-folder-version-message" }, cancellationToken: token);
        var record = new HandOffDocumentFolderVersionMessageRecord
        {
            Message = found.Outcome.Message,
            Status = found.Result.Status,
            Name = found.DocumentFolder.Name,
            DocumentFolderId = found.DocumentFolder.DocumentFolderId,
        };
        await DispatchHandOffDocumentFolderVersionMessageAsync("candidate-approval", record, channel, token);
    }

    private static async Task DispatchHandOffDocumentFolderVersionMessageAsync(string kind, HandOffDocumentFolderVersionMessageRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<HandOffDocumentFolderVersionMessageRecord, CancellationToken, Task> write = new HandOffDocumentFolderVersionMessageLogOnly().DeliverAsync;
        switch (kind)
        {
            case "onboarding-plan-workflow":
                write = new HandOffDocumentFolderVersionMessageOnboardingPlanWorkflowSink(new ApiOnboarding.OnboardingPlanWorkflowService.OnboardingPlanWorkflowServiceClient(channel)).DeliverAsync;
                break;
            case "route-binding-query":
                write = new HandOffDocumentFolderVersionMessageRouteBindingQuerySink(new ApiEdgegateway.RouteBindingQueryService.RouteBindingQueryServiceClient(channel)).DeliverAsync;
                break;
            case "candidate-approval":
                write = new HandOffDocumentFolderVersionMessageCandidateApprovalTarget(new ApiRecruiting.CandidateApprovalService.CandidateApprovalServiceClient(channel)).DeliverAsync;
                break;
        }
        await write(record, token);
    }
}
