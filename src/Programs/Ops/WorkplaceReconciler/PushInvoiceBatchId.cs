using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCommon = Acme.Common.V1;
using ApiConcessions = Acme.Concessions.V1;
using ApiInvoicing = Acme.Invoicing.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.WorkplaceReconciler;

/// <summary>Where PushInvoiceBatchId hands each record.</summary>
internal interface IPushInvoiceBatchIdWriter
{
    Task WriteAsync(PushInvoiceBatchIdRecord record, CancellationToken token);
}

/// <summary>What PushInvoiceBatchId keeps of each CalculateInvoiceBatch response.</summary>
internal sealed class PushInvoiceBatchIdRecord
{
    public string InvoiceBatchId { get; init; } = "";
    public string Notes { get; init; } = "";
    public string CurrencyCode { get; init; } = "";
    public string Number { get; init; } = "";
}

/// <summary>Passes records to acme.concessions.v1.ConcessionStandCalculationService.</summary>
internal sealed class PushInvoiceBatchIdConcessionStandCalculationWriter(ApiConcessions.ConcessionStandCalculationService.ConcessionStandCalculationServiceClient client) : IPushInvoiceBatchIdWriter
{
    public async Task WriteAsync(PushInvoiceBatchIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiConcessions.SimulateConcessionStandRequest { ConcessionStandId = record.Notes, Reason = record.CurrencyCode, Details = new ApiConcessions.SimulateConcessionStandRequest.Types.Details { CurrencyCode = record.InvoiceBatchId } }, token, client.SimulateConcessionStandAsync);
    }
}

/// <summary>Hands records to acme.workplaces.v1.BuildingService.</summary>
internal sealed class PushInvoiceBatchIdBuildingWriter : IPushInvoiceBatchIdWriter
{
    private readonly ApiWorkplaces.BuildingService.BuildingServiceClient _client;

    public PushInvoiceBatchIdBuildingWriter(ApiWorkplaces.BuildingService.BuildingServiceClient client) => _client = client;

    public async Task WriteAsync(PushInvoiceBatchIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiWorkplaces.ListBuildingsRequest { Filter = new ApiWorkplaces.ListBuildingsRequest.Types.Filter { OwnerId = record.Notes }, Page = new ApiCommon.PageRequest { PageToken = record.CurrencyCode } }, token, _client.ListBuildingsAsync);
    }
}

/// <summary>Hands records to acme.timekeeping.v1.OvertimeWorkflowService.</summary>
internal sealed class PushInvoiceBatchIdOvertimeWorkflowWriter(ApiTimekeeping.OvertimeWorkflowService.OvertimeWorkflowServiceClient client) : IPushInvoiceBatchIdWriter
{
    public async Task WriteAsync(PushInvoiceBatchIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTimekeeping.PauseOvertimeWorkflowRequest { Overtime = new ApiTimekeeping.Overtime { Description = record.CurrencyCode } }, token, client.PauseOvertimeWorkflowAsync);
    }
}

/// <summary>Reads acme.invoicing.v1.InvoiceBatchCalculationService/CalculateInvoiceBatch and hands what it keeps to the sink of its kind.</summary>
internal static class PushInvoiceBatchId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiInvoicing.InvoiceBatchCalculationService.InvoiceBatchCalculationServiceClient(channel);
        var got = await source.CalculateInvoiceBatchAsync(new ApiInvoicing.CalculateInvoiceBatchRequest() { Reason = "nightly-push-invoice-batch-id" }, cancellationToken: token);
        var record = new PushInvoiceBatchIdRecord
        {
            InvoiceBatchId = got.InvoiceBatch.InvoiceBatchId,
            Notes = got.InvoiceBatch.Notes,
            CurrencyCode = got.Breakdown.CurrencyCode,
            Number = got.InvoiceBatch.Number,
        };
        var sinks = new Dictionary<string, IPushInvoiceBatchIdWriter>
        {
            ["concession-stand-calculation"] = new PushInvoiceBatchIdConcessionStandCalculationWriter(new ApiConcessions.ConcessionStandCalculationService.ConcessionStandCalculationServiceClient(channel)),
            ["building"] = new PushInvoiceBatchIdBuildingWriter(new ApiWorkplaces.BuildingService.BuildingServiceClient(channel)),
            ["overtime-workflow"] = new PushInvoiceBatchIdOvertimeWorkflowWriter(new ApiTimekeeping.OvertimeWorkflowService.OvertimeWorkflowServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "overtime-workflow")
                continue;
            await sink.WriteAsync(record, token);
        }
    }
}
