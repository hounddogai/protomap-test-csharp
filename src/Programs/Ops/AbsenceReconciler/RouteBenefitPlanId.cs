using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiBenefits = Acme.Benefits.V1;
using ApiSeating = Acme.Seating.V1;

namespace Acme.Workforce.Ops.AbsenceReconciler;

/// <summary>What RouteBenefitPlanId keeps of each RejectBenefitPlan response.</summary>
internal sealed class RouteBenefitPlanIdRecord
{
    public string BenefitPlanId { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Name { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Sends records to acme.seating.v1.SeatMapQueryService.</summary>
internal sealed class RouteBenefitPlanIdSeatMapQueryWriter
{
    private readonly ApiSeating.SeatMapQueryService.SeatMapQueryServiceClient _client;

    public RouteBenefitPlanIdSeatMapQueryWriter(ApiSeating.SeatMapQueryService.SeatMapQueryServiceClient client) => _client = client;

    public async Task WriteAsync(RouteBenefitPlanIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiSeating.SummarizeSeatMapsRequest { SeatMapId = record.Message, Details = new ApiSeating.SummarizeSeatMapsRequest.Types.Details { Cron = record.Name, Format = record.BenefitPlanId } }, token, _client.SummarizeSeatMapsAsync);
    }
}

/// <summary>Reads acme.benefits.v1.BenefitPlanApprovalService/RejectBenefitPlan and hands what it keeps to acme.seating.v1.SeatMapQueryService.</summary>
internal static class RouteBenefitPlanId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiBenefits.BenefitPlanApprovalService.BenefitPlanApprovalServiceClient(channel);
        var got = await source.RejectBenefitPlanAsync(new ApiBenefits.RejectBenefitPlanRequest() { BenefitPlanId = "main-route-benefit-plan-id" }, cancellationToken: token);
        var record = new RouteBenefitPlanIdRecord
        {
            BenefitPlanId = got.BenefitPlan.BenefitPlanId,
            CorrelationId = got.Result.CorrelationId,
            Name = got.BenefitPlan.Name,
            Message = got.Result.Message,
        };
        await new RouteBenefitPlanIdSeatMapQueryWriter(new ApiSeating.SeatMapQueryService.SeatMapQueryServiceClient(channel)).WriteAsync(record, token);
    }
}
