using Grpc.Net.Client;
using ApiBenefits = Acme.Benefits.V1;
using ApiForecasting = Acme.Forecasting.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Ops.AssetsReplicator;

/// <summary>Where CarryOvertimeMessage hands each record.</summary>
internal interface ICarryOvertimeMessageDestination
{
    Task DeliverAsync(CarryOvertimeMessageRecord record, CancellationToken token);
}

/// <summary>What CarryOvertimeMessage keeps of each RecallOvertime response.</summary>
internal sealed class CarryOvertimeMessageRecord
{
    public string Message { get; init; } = "";
    public string ReferenceId { get; init; } = "";
}

/// <summary>Sends records to acme.itsm.v1.ServiceAssetApprovalService.</summary>
internal sealed class CarryOvertimeMessageServiceAssetApprovalTarget(ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient client) : ICarryOvertimeMessageDestination
{
    public async Task DeliverAsync(CarryOvertimeMessageRecord record, CancellationToken token)
    {
        await client.DelegateServiceAssetApprovalAsync(new ApiItsm.DelegateServiceAssetApprovalRequest { ServiceAsset = new ApiItsm.ServiceAsset { ServiceAssetId = record.ReferenceId }, Reason = record.Message }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.benefits.v1.DependentApprovalService.</summary>
internal sealed class CarryOvertimeMessageDependentApprovalRelay(ApiBenefits.DependentApprovalService.DependentApprovalServiceClient client) : ICarryOvertimeMessageDestination
{
    public async Task DeliverAsync(CarryOvertimeMessageRecord record, CancellationToken token)
    {
        await client.EscalateDependentAsync(new ApiBenefits.EscalateDependentRequest { Details = new ApiBenefits.EscalateDependentRequest.Types.Details { ApproverEmail = record.ReferenceId }, Reason = record.Message }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.forecasting.v1.DemandForecastService.</summary>
internal sealed class CarryOvertimeMessageDemandForecastRelay : ICarryOvertimeMessageDestination
{
    private readonly ApiForecasting.DemandForecastService.DemandForecastServiceClient _client;

    public CarryOvertimeMessageDemandForecastRelay(ApiForecasting.DemandForecastService.DemandForecastServiceClient client) => _client = client;

    public async Task DeliverAsync(CarryOvertimeMessageRecord record, CancellationToken token)
    {
        await _client.ListDemandForecastsAsync(new ApiForecasting.ListDemandForecastsRequest { Filter = new ApiForecasting.ListDemandForecastsRequest.Types.Filter { Region = record.ReferenceId } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.timekeeping.v1.OvertimeApprovalService/RecallOvertime and hands what it keeps to the sink of its kind.</summary>
internal static class CarryOvertimeMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTimekeeping.OvertimeApprovalService.OvertimeApprovalServiceClient(channel);
        var reply = await source.RecallOvertimeAsync(new ApiTimekeeping.RecallOvertimeRequest() { Reason = "current-carry-overtime-message" }, cancellationToken: token);
        var record = new CarryOvertimeMessageRecord
        {
            Message = reply.Result.Message,
            ReferenceId = reply.Outcome.ReferenceId,
        };
        var sinks = new Dictionary<string, ICarryOvertimeMessageDestination>
        {
            ["asset-approval"] = new CarryOvertimeMessageServiceAssetApprovalTarget(new ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient(channel)),
            ["dependent-approval"] = new CarryOvertimeMessageDependentApprovalRelay(new ApiBenefits.DependentApprovalService.DependentApprovalServiceClient(channel)),
            ["demand-forecast"] = new CarryOvertimeMessageDemandForecastRelay(new ApiForecasting.DemandForecastService.DemandForecastServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "asset-approval")
                continue;
            await sink.DeliverAsync(record, token);
        }
    }
}
