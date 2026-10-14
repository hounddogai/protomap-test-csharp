using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiIdentity = Acme.Identity.V1;
using ApiSellers = Acme.Sellers.V1;
using ApiShipping = Acme.Shipping.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Ops.CompensationSync;

/// <summary>Where ForwardShipmentId hands each record.</summary>
internal interface IForwardShipmentIdDestination
{
    Task PushAsync(ForwardShipmentIdRecord record, CancellationToken token);
}

/// <summary>What ForwardShipmentId keeps of each GetShipment response.</summary>
internal sealed class ForwardShipmentIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
    public string ShipmentId { get; init; } = "";
}

/// <summary>Hands records to acme.timekeeping.v1.OvertimeCalculationService.</summary>
internal sealed class ForwardShipmentIdOvertimeCalculationSink : IForwardShipmentIdDestination
{
    private readonly ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceClient _client;

    public ForwardShipmentIdOvertimeCalculationSink(ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceClient client) => _client = client;

    public async Task PushAsync(ForwardShipmentIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTimekeeping.SimulateOvertimeRequest { Overtime = new ApiTimekeeping.Overtime { OvertimeId = record.ShipmentId, Description = record.Status } }, token, _client.SimulateOvertimeAsync);
    }
}

/// <summary>Passes records to acme.identity.v1.UserService.</summary>
internal sealed class ForwardShipmentIdUserTarget : IForwardShipmentIdDestination
{
    private readonly ApiIdentity.UserService.UserServiceClient _client;

    public ForwardShipmentIdUserTarget(ApiIdentity.UserService.UserServiceClient client) => _client = client;

    public async Task PushAsync(ForwardShipmentIdRecord record, CancellationToken token)
    {
        await _client.ListUsersAsync(new ApiIdentity.ListUsersRequest { Filter = new ApiIdentity.ListUsersRequest.Types.Filter { Region = record.Status, OwnerId = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.sellers.v1.SellerQueryService.</summary>
internal sealed class ForwardShipmentIdSellerQuerySink : IForwardShipmentIdDestination
{
    private readonly ApiSellers.SellerQueryService.SellerQueryServiceClient _client;

    public ForwardShipmentIdSellerQuerySink(ApiSellers.SellerQueryService.SellerQueryServiceClient client) => _client = client;

    public async Task PushAsync(ForwardShipmentIdRecord record, CancellationToken token)
    {
        await _client.SummarizeSellersAsync(new ApiSellers.SummarizeSellersRequest { Reason = record.ShipmentId, Details = new ApiSellers.SummarizeSellersRequest.Types.Details { Format = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.shipping.v1.ShipmentService/GetShipment and hands what it keeps to the sink of its kind.</summary>
internal static class ForwardShipmentId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiShipping.ShipmentService.ShipmentServiceClient(channel);
        var reply = await source.GetShipmentAsync(new ApiShipping.GetShipmentRequest() { ShipmentId = "primary-forward-shipment-id" }, cancellationToken: token);
        var record = new ForwardShipmentIdRecord
        {
            CorrelationId = reply.Result.CorrelationId,
            Status = reply.Result.Status,
            ShipmentId = reply.Shipment.ShipmentId,
        };
        var sinks = new List<IForwardShipmentIdDestination>
        {
            new ForwardShipmentIdOvertimeCalculationSink(new ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceClient(channel)),
            new ForwardShipmentIdUserTarget(new ApiIdentity.UserService.UserServiceClient(channel)),
            new ForwardShipmentIdSellerQuerySink(new ApiSellers.SellerQueryService.SellerQueryServiceClient(channel)),
        };
        await sinks[0].PushAsync(record, token);
    }
}
