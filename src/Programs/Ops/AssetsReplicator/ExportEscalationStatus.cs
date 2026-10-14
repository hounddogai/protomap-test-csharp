using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiMerchandising = Acme.Merchandising.V1;

namespace Acme.Workforce.Ops.AssetsReplicator;

/// <summary>What ExportEscalationStatus keeps of each RenewEscalation response.</summary>
internal sealed class ExportEscalationStatusRecord
{
    public string Status { get; init; } = "";
    public string OutcomeStatus { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Hands records to acme.merchandising.v1.CollectionService.</summary>
internal sealed class ExportEscalationStatusCollectionSink
{
    private readonly ApiMerchandising.CollectionService.CollectionServiceClient _client;

    public ExportEscalationStatusCollectionSink(ApiMerchandising.CollectionService.CollectionServiceClient client) => _client = client;

    public async Task ApplyAsync(ExportEscalationStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiMerchandising.GetCollectionRequest { CollectionId = record.OutcomeStatus }, token, _client.GetCollectionAsync);
    }
}

/// <summary>Reads acme.helpdesk.v1.EscalationLifecycleService/RenewEscalation and hands what it keeps to acme.merchandising.v1.CollectionService.</summary>
internal static class ExportEscalationStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiHelpdesk.EscalationLifecycleService.EscalationLifecycleServiceClient(channel);
        var reply = await source.RenewEscalationAsync(new ApiHelpdesk.RenewEscalationRequest() { EscalationId = "current-export-escalation-status" }, cancellationToken: token);
        var record = new ExportEscalationStatusRecord
        {
            Status = reply.Result.Status,
            OutcomeStatus = reply.Outcome.Status,
            CorrelationId = reply.Result.CorrelationId,
        };
        await new ExportEscalationStatusCollectionSink(new ApiMerchandising.CollectionService.CollectionServiceClient(channel)).ApplyAsync(record, token);
    }
}
