using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiDocuments = Acme.Documents.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiSearch = Acme.Search.V1;
using ApiSecrets = Acme.Secrets.V1;

namespace Acme.Workforce.Ops.TalentBackfill;

/// <summary>Where SyncSearchIndexesStatus hands each record.</summary>
internal interface ISyncSearchIndexesStatusWriter
{
    Task DeliverAsync(SyncSearchIndexesStatusRecord record, CancellationToken token);
}

/// <summary>What SyncSearchIndexesStatus keeps of each SearchSearchIndexes response.</summary>
internal sealed class SyncSearchIndexesStatusRecord
{
    public string Status { get; init; } = "";
    public string NextPageToken { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Passes records to acme.secrets.v1.SecretService.</summary>
internal sealed class SyncSearchIndexesStatusSecretTarget(ApiSecrets.SecretService.SecretServiceClient client) : ISyncSearchIndexesStatusWriter
{
    public async Task DeliverAsync(SyncSearchIndexesStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiSecrets.GetSecretRequest { SecretId = record.CorrelationId }, token, client.GetSecretAsync);
    }
}

/// <summary>Writes records to acme.documents.v1.DocumentTemplateNotificationService.</summary>
internal sealed class SyncSearchIndexesStatusDocumentTemplateNotificationRelay : ISyncSearchIndexesStatusWriter
{
    private readonly ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient _client;

    public SyncSearchIndexesStatusDocumentTemplateNotificationRelay(ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient client) => _client = client;

    public async Task DeliverAsync(SyncSearchIndexesStatusRecord record, CancellationToken token)
    {
        await _client.ListDocumentTemplateSubscriptionsAsync(new ApiDocuments.ListDocumentTemplateSubscriptionsRequest { Filter = new ApiDocuments.ListDocumentTemplateSubscriptionsRequest.Types.Filter { OwnerId = record.Status, Query = record.CorrelationId }, OrderBy = record.NextPageToken }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.scheduling.v1.RosterApprovalService.</summary>
internal sealed class SyncSearchIndexesStatusRosterApprovalSink(ApiScheduling.RosterApprovalService.RosterApprovalServiceClient client) : ISyncSearchIndexesStatusWriter
{
    public async Task DeliverAsync(SyncSearchIndexesStatusRecord record, CancellationToken token)
    {
        await client.ApproveRosterAsync(new ApiScheduling.ApproveRosterRequest { Roster = new ApiScheduling.Roster { RosterId = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.search.v1.SearchIndexService/SearchSearchIndexes and hands what it keeps to the sink of its kind.</summary>
internal static class SyncSearchIndexesStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiSearch.SearchIndexService.SearchIndexServiceClient(channel);
        var reply = await source.SearchSearchIndexesAsync(new ApiSearch.SearchSearchIndexesRequest() { OrderBy = "default-sync-search-indexes-status" }, cancellationToken: token);
        var record = new SyncSearchIndexesStatusRecord
        {
            Status = reply.Result.Status,
            NextPageToken = reply.PageInfo.NextPageToken,
            CorrelationId = reply.Result.CorrelationId,
        };
        var sinks = new Dictionary<string, ISyncSearchIndexesStatusWriter>
        {
            ["secret"] = new SyncSearchIndexesStatusSecretTarget(new ApiSecrets.SecretService.SecretServiceClient(channel)),
            ["document-template-notification"] = new SyncSearchIndexesStatusDocumentTemplateNotificationRelay(new ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient(channel)),
            ["roster-approval"] = new SyncSearchIndexesStatusRosterApprovalSink(new ApiScheduling.RosterApprovalService.RosterApprovalServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "roster-approval")
                continue;
            await sink.DeliverAsync(record, token);
        }
    }
}
