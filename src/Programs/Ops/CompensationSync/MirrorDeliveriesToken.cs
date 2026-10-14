using Grpc.Net.Client;
using ApiDeliveries = Acme.Deliveries.V1;
using ApiDevices = Acme.Devices.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiKeymanagement = Acme.Keymanagement.V1;

namespace Acme.Workforce.Ops.CompensationSync;

/// <summary>Where MirrorDeliveriesToken hands each record.</summary>
internal interface IMirrorDeliveriesTokenSink
{
    Task SendAsync(MirrorDeliveriesTokenRecord record, CancellationToken token);
}

/// <summary>What MirrorDeliveriesToken keeps of each FindDeliveries response.</summary>
internal sealed class MirrorDeliveriesTokenRecord
{
    public string NextPageToken { get; init; } = "";
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Hands records to acme.itsm.v1.ChangeRequestCalculationService.</summary>
internal sealed class MirrorDeliveriesTokenChangeRequestCalculationWriter : IMirrorDeliveriesTokenSink
{
    private readonly ApiItsm.ChangeRequestCalculationService.ChangeRequestCalculationServiceClient _client;

    public MirrorDeliveriesTokenChangeRequestCalculationWriter(ApiItsm.ChangeRequestCalculationService.ChangeRequestCalculationServiceClient client) => _client = client;

    public async Task SendAsync(MirrorDeliveriesTokenRecord record, CancellationToken token)
    {
        await _client.RecalculateChangeRequestsAsync(new ApiItsm.RecalculateChangeRequestsRequest { Reason = record.Status }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.keymanagement.v1.CryptoKeyCalculationService.</summary>
internal sealed class MirrorDeliveriesTokenCryptoKeyCalculationRelay : IMirrorDeliveriesTokenSink
{
    private readonly ApiKeymanagement.CryptoKeyCalculationService.CryptoKeyCalculationServiceClient _client;

    public MirrorDeliveriesTokenCryptoKeyCalculationRelay(ApiKeymanagement.CryptoKeyCalculationService.CryptoKeyCalculationServiceClient client) => _client = client;

    public async Task SendAsync(MirrorDeliveriesTokenRecord record, CancellationToken token)
    {
        await _client.EstimateCryptoKeyAsync(new ApiKeymanagement.EstimateCryptoKeyRequest { Reason = record.Message }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.devices.v1.DevicePolicyApprovalService.</summary>
internal sealed class MirrorDeliveriesTokenDevicePolicyApprovalWriter : IMirrorDeliveriesTokenSink
{
    private readonly ApiDevices.DevicePolicyApprovalService.DevicePolicyApprovalServiceClient _client;

    public MirrorDeliveriesTokenDevicePolicyApprovalWriter(ApiDevices.DevicePolicyApprovalService.DevicePolicyApprovalServiceClient client) => _client = client;

    public async Task SendAsync(MirrorDeliveriesTokenRecord record, CancellationToken token)
    {
        await _client.DelegateDevicePolicyApprovalAsync(new ApiDevices.DelegateDevicePolicyApprovalRequest { DevicePolicy = new ApiDevices.DevicePolicy { DevicePolicyId = record.Message, Name = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.deliveries.v1.DeliveryQueryService/FindDeliveries and hands what it keeps to the sink of its kind.</summary>
internal static class MirrorDeliveriesToken
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDeliveries.DeliveryQueryService.DeliveryQueryServiceClient(channel);
        var reply = await source.FindDeliveriesAsync(new ApiDeliveries.FindDeliveriesRequest() { OrderBy = "live-mirror-deliveries-token" }, cancellationToken: token);
        var record = new MirrorDeliveriesTokenRecord
        {
            NextPageToken = reply.PageInfo.NextPageToken,
            Message = reply.Result.Message,
            Status = reply.Result.Status,
            CorrelationId = reply.Result.CorrelationId,
        };
        await ActOnMirrorDeliveriesTokenAsync("change-request-calculation", record, channel, token);
    }

    private static async Task ActOnMirrorDeliveriesTokenAsync(string kind, MirrorDeliveriesTokenRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["change-request-calculation"] = () => new MirrorDeliveriesTokenChangeRequestCalculationWriter(new ApiItsm.ChangeRequestCalculationService.ChangeRequestCalculationServiceClient(channel)).SendAsync(record, token),
            ["crypto-key-calculation"] = () => new MirrorDeliveriesTokenCryptoKeyCalculationRelay(new ApiKeymanagement.CryptoKeyCalculationService.CryptoKeyCalculationServiceClient(channel)).SendAsync(record, token),
            ["device-policy-approval"] = () => new MirrorDeliveriesTokenDevicePolicyApprovalWriter(new ApiDevices.DevicePolicyApprovalService.DevicePolicyApprovalServiceClient(channel)).SendAsync(record, token),
        };
        await actions[kind]();
    }
}
