using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiLabeling = Acme.Labeling.V1;
using ApiTicketing = Acme.Ticketing.V1;

namespace Acme.Workforce.Ops.TalentBackfill;

/// <summary>What SyncTicketId keeps of each GetTicket response.</summary>
internal sealed class SyncTicketIdRecord
{
    public string TicketId { get; init; } = "";
    public string Notes { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Writes records to acme.labeling.v1.LabelingTaskCalculationService.</summary>
internal sealed class SyncTicketIdLabelingTaskCalculationTarget(ApiLabeling.LabelingTaskCalculationService.LabelingTaskCalculationServiceClient client)
{
    public async Task ForwardAsync(SyncTicketIdRecord record, CancellationToken token)
    {
        await client.EstimateLabelingTaskAsync(new ApiLabeling.EstimateLabelingTaskRequest { LabelingTask = new ApiLabeling.LabelingTask { LabelingTaskId = record.Status }, Reason = record.Notes, LabelingTaskId = record.TicketId }, cancellationToken: token);
    }
}

/// <summary>Reads acme.ticketing.v1.TicketService/GetTicket and hands what it keeps to acme.labeling.v1.LabelingTaskCalculationService.</summary>
internal static class SyncTicketId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTicketing.TicketService.TicketServiceClient(channel);
        var found = await source.GetTicketAsync(new ApiTicketing.GetTicketRequest() { TicketId = "live-sync-ticket-id" }, cancellationToken: token);
        var record = new SyncTicketIdRecord
        {
            TicketId = Text.Normalize(found.Ticket.TicketId),
            Notes = found.Ticket.Notes,
            Status = found.Result.Status,
        };
        await new SyncTicketIdLabelingTaskCalculationTarget(new ApiLabeling.LabelingTaskCalculationService.LabelingTaskCalculationServiceClient(channel)).ForwardAsync(record, token);
    }
}
