using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiProcurement = Acme.Procurement.V1;
using ApiRisk = Acme.Risk.V1;

namespace Acme.Workforce.Ops.TalentBackfill;

/// <summary>What RouteRiskRegistersId keeps of each SearchRiskRegisters response.</summary>
internal sealed class RouteRiskRegistersIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
    public string NextPageToken { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Sends records to acme.procurement.v1.PurchaseRequisitionCalculationService.</summary>
internal sealed class RouteRiskRegistersIdPurchaseRequisitionCalculationRelay(ApiProcurement.PurchaseRequisitionCalculationService.PurchaseRequisitionCalculationServiceClient client)
{
    public async Task DeliverAsync(RouteRiskRegistersIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiProcurement.QuotePurchaseRequisitionRequest { Reason = record.Status, Details = new ApiProcurement.QuotePurchaseRequisitionRequest.Types.Details { CurrencyCode = record.NextPageToken } }, token, client.QuotePurchaseRequisitionAsync);
    }
}

/// <summary>Reads acme.risk.v1.RiskRegisterService/SearchRiskRegisters and hands what it keeps to acme.procurement.v1.PurchaseRequisitionCalculationService.</summary>
internal static class RouteRiskRegistersId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiRisk.RiskRegisterService.RiskRegisterServiceClient(channel);
        var got = await source.SearchRiskRegistersAsync(new ApiRisk.SearchRiskRegistersRequest() { OrderBy = "current-route-risk-registers-id" }, cancellationToken: token);
        var record = new RouteRiskRegistersIdRecord
        {
            CorrelationId = got.Result.CorrelationId,
            Message = got.Result.Message,
            NextPageToken = got.PageInfo.NextPageToken,
            Status = got.Result.Status,
        };
        await new RouteRiskRegistersIdPurchaseRequisitionCalculationRelay(new ApiProcurement.PurchaseRequisitionCalculationService.PurchaseRequisitionCalculationServiceClient(channel)).DeliverAsync(record, token);
    }
}
