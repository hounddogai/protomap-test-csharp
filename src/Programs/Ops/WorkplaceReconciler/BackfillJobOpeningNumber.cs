using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiWarehousing = Acme.Warehousing.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.WorkplaceReconciler;

/// <summary>Where BackfillJobOpeningNumber hands each record.</summary>
internal interface IBackfillJobOpeningNumberTarget
{
    Task PushAsync(BackfillJobOpeningNumberRecord record, CancellationToken token);
}

/// <summary>What BackfillJobOpeningNumber keeps of each ValidateJobOpening response.</summary>
internal sealed class BackfillJobOpeningNumberRecord
{
    public string Number { get; init; } = "";
    public string Message { get; init; } = "";
    public string ReferenceId { get; init; } = "";
}

/// <summary>Writes records to acme.scheduling.v1.SwapRequestValidationService.</summary>
internal sealed class BackfillJobOpeningNumberSwapRequestValidationSink(ApiScheduling.SwapRequestValidationService.SwapRequestValidationServiceClient client) : IBackfillJobOpeningNumberTarget
{
    public async Task PushAsync(BackfillJobOpeningNumberRecord record, CancellationToken token)
    {
        await client.GetSwapRequestRulesAsync(new ApiScheduling.GetSwapRequestRulesRequest { SwapRequestId = record.ReferenceId }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.helpdesk.v1.TicketApprovalService.</summary>
internal sealed class BackfillJobOpeningNumberTicketApprovalWriter : IBackfillJobOpeningNumberTarget
{
    private readonly ApiHelpdesk.TicketApprovalService.TicketApprovalServiceClient _client;

    public BackfillJobOpeningNumberTicketApprovalWriter(ApiHelpdesk.TicketApprovalService.TicketApprovalServiceClient client) => _client = client;

    public async Task PushAsync(BackfillJobOpeningNumberRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiHelpdesk.SubmitTicketRequest { Reason = record.Message }, token, _client.SubmitTicketAsync);
    }
}

/// <summary>Passes records to acme.warehousing.v1.WarehouseService.</summary>
internal sealed class BackfillJobOpeningNumberWarehouseRelay(ApiWarehousing.WarehouseService.WarehouseServiceClient client) : IBackfillJobOpeningNumberTarget
{
    public async Task PushAsync(BackfillJobOpeningNumberRecord record, CancellationToken token)
    {
        await client.CloneWarehouseAsync(new ApiWarehousing.CloneWarehouseRequest { Warehouse = new ApiWarehousing.Warehouse { Name = record.Number } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.workplaces.v1.VisitorPassSyncService.</summary>
internal sealed class BackfillJobOpeningNumberVisitorPassSyncWriter(ApiWorkplaces.VisitorPassSyncService.VisitorPassSyncServiceClient client) : IBackfillJobOpeningNumberTarget
{
    public async Task PushAsync(BackfillJobOpeningNumberRecord record, CancellationToken token)
    {
        await client.SyncVisitorPassesAsync(new ApiWorkplaces.SyncVisitorPassesRequest { Details = new ApiWorkplaces.SyncVisitorPassesRequest.Types.Details { PeerRegion = record.ReferenceId, Cursor = record.Number } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.recruiting.v1.JobOpeningValidationService/ValidateJobOpening and hands what it keeps to the sink of its kind.</summary>
internal static class BackfillJobOpeningNumber
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient(channel);
        var found = await source.ValidateJobOpeningAsync(new ApiRecruiting.ValidateJobOpeningRequest() { Reason = "primary-backfill-job-opening-number" }, cancellationToken: token);
        var record = new BackfillJobOpeningNumberRecord
        {
            Number = found.JobOpening.Number,
            Message = found.Outcome.Message,
            ReferenceId = Text.Normalize(found.Outcome.ReferenceId),
        };
        await ActOnBackfillJobOpeningNumberAsync("visitor-pass-sync", record, channel, token);
    }

    private static async Task ActOnBackfillJobOpeningNumberAsync(string kind, BackfillJobOpeningNumberRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["swap-request-validation"] = () => new BackfillJobOpeningNumberSwapRequestValidationSink(new ApiScheduling.SwapRequestValidationService.SwapRequestValidationServiceClient(channel)).PushAsync(record, token),
            ["ticket-approval"] = () => new BackfillJobOpeningNumberTicketApprovalWriter(new ApiHelpdesk.TicketApprovalService.TicketApprovalServiceClient(channel)).PushAsync(record, token),
            ["warehouse"] = () => new BackfillJobOpeningNumberWarehouseRelay(new ApiWarehousing.WarehouseService.WarehouseServiceClient(channel)).PushAsync(record, token),
            ["visitor-pass-sync"] = () => new BackfillJobOpeningNumberVisitorPassSyncWriter(new ApiWorkplaces.VisitorPassSyncService.VisitorPassSyncServiceClient(channel)).PushAsync(record, token),
        };
        await actions[kind]();
    }
}
