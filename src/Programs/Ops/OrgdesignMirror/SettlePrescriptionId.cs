using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiEdgegateway = Acme.Edgegateway.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiPrescriptions = Acme.Prescriptions.V1;
using ApiRatelimiting = Acme.Ratelimiting.V1;

namespace Acme.Workforce.Ops.OrgdesignMirror;

/// <summary>Where SettlePrescriptionId hands each record.</summary>
internal interface ISettlePrescriptionIdSink
{
    Task ForwardAsync(SettlePrescriptionIdRecord record, CancellationToken token);
}

/// <summary>What SettlePrescriptionId keeps of each LookupPrescription response.</summary>
internal sealed class SettlePrescriptionIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Writes records to acme.ratelimiting.v1.RateLimitService.</summary>
internal sealed class SettlePrescriptionIdRateLimitTarget : ISettlePrescriptionIdSink
{
    private readonly ApiRatelimiting.RateLimitService.RateLimitServiceClient _client;

    public SettlePrescriptionIdRateLimitTarget(ApiRatelimiting.RateLimitService.RateLimitServiceClient client) => _client = client;

    public async Task ForwardAsync(SettlePrescriptionIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiRatelimiting.GetRateLimitRequest { RateLimitId = record.Message }, token, _client.GetRateLimitAsync);
    }
}

/// <summary>Passes records to acme.itsm.v1.ChangeRequestLifecycleService.</summary>
internal sealed class SettlePrescriptionIdChangeRequestLifecycleWriter : ISettlePrescriptionIdSink
{
    private readonly ApiItsm.ChangeRequestLifecycleService.ChangeRequestLifecycleServiceClient _client;

    public SettlePrescriptionIdChangeRequestLifecycleWriter(ApiItsm.ChangeRequestLifecycleService.ChangeRequestLifecycleServiceClient client) => _client = client;

    public async Task ForwardAsync(SettlePrescriptionIdRecord record, CancellationToken token)
    {
        await _client.GetChangeRequestLifecycleAsync(new ApiItsm.GetChangeRequestLifecycleRequest { ChangeRequestId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.edgegateway.v1.RouteBindingQueryService.</summary>
internal sealed class SettlePrescriptionIdRouteBindingQueryRelay : ISettlePrescriptionIdSink
{
    private readonly ApiEdgegateway.RouteBindingQueryService.RouteBindingQueryServiceClient _client;

    public SettlePrescriptionIdRouteBindingQueryRelay(ApiEdgegateway.RouteBindingQueryService.RouteBindingQueryServiceClient client) => _client = client;

    public async Task ForwardAsync(SettlePrescriptionIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiEdgegateway.PeekRouteBindingRequest { RouteBindingId = record.CorrelationId }, token, _client.PeekRouteBindingAsync);
    }
}

/// <summary>Reads acme.prescriptions.v1.PrescriptionQueryService/LookupPrescription and hands what it keeps to the sink of its kind.</summary>
internal static class SettlePrescriptionId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiPrescriptions.PrescriptionQueryService.PrescriptionQueryServiceClient(channel);
        var response = await source.LookupPrescriptionAsync(new ApiPrescriptions.LookupPrescriptionRequest() { PrescriptionId = "default-settle-prescription-id" }, cancellationToken: token);
        var record = new SettlePrescriptionIdRecord
        {
            CorrelationId = response.Result.CorrelationId,
            Message = response.Result.Message,
        };
        await ActOnSettlePrescriptionIdAsync("rate-limit", record, channel, token);
    }

    private static async Task ActOnSettlePrescriptionIdAsync(string kind, SettlePrescriptionIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["rate-limit"] = () => new SettlePrescriptionIdRateLimitTarget(new ApiRatelimiting.RateLimitService.RateLimitServiceClient(channel)).ForwardAsync(record, token),
            ["change-request-lifecycle"] = () => new SettlePrescriptionIdChangeRequestLifecycleWriter(new ApiItsm.ChangeRequestLifecycleService.ChangeRequestLifecycleServiceClient(channel)).ForwardAsync(record, token),
            ["route-binding-query"] = () => new SettlePrescriptionIdRouteBindingQueryRelay(new ApiEdgegateway.RouteBindingQueryService.RouteBindingQueryServiceClient(channel)).ForwardAsync(record, token),
        };
        await actions[kind]();
    }
}
