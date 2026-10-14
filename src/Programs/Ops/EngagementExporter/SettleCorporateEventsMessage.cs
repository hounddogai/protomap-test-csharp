using Grpc.Net.Client;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiRouting = Acme.Routing.V1;

namespace Acme.Workforce.Ops.EngagementExporter;

/// <summary>Where SettleCorporateEventsMessage hands each record.</summary>
internal interface ISettleCorporateEventsMessageDestination
{
    Task PushAsync(SettleCorporateEventsMessageRecord record, CancellationToken token);
}

/// <summary>What SettleCorporateEventsMessage keeps of each ResolveCorporateEvents response.</summary>
internal sealed class SettleCorporateEventsMessageRecord
{
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
    public string NextPageToken { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Passes records to acme.routing.v1.RouteQueryService.</summary>
internal sealed class SettleCorporateEventsMessageRouteQueryTarget(ApiRouting.RouteQueryService.RouteQueryServiceClient client) : ISettleCorporateEventsMessageDestination
{
    public async Task PushAsync(SettleCorporateEventsMessageRecord record, CancellationToken token)
    {
        await client.SuggestRoutesAsync(new ApiRouting.SuggestRoutesRequest { Filter = new ApiRouting.SuggestRoutesRequest.Types.Filter { OwnerId = record.NextPageToken } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.devices.v1.DevicePolicyValidationService.</summary>
internal sealed class SettleCorporateEventsMessageDevicePolicyValidationWriter : ISettleCorporateEventsMessageDestination
{
    private readonly ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient _client;

    public SettleCorporateEventsMessageDevicePolicyValidationWriter(ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient client) => _client = client;

    public async Task PushAsync(SettleCorporateEventsMessageRecord record, CancellationToken token)
    {
        await _client.PreviewDevicePolicyAsync(new ApiDevices.PreviewDevicePolicyRequest { Reason = record.Status, Details = new ApiDevices.PreviewDevicePolicyRequest.Types.Details { Locale = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.corporateevents.v1.CorporateEventQueryService/ResolveCorporateEvents and hands what it keeps to the sink of its kind.</summary>
internal static class SettleCorporateEventsMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiCorporateevents.CorporateEventQueryService.CorporateEventQueryServiceClient(channel);
        var got = await source.ResolveCorporateEventsAsync(new ApiCorporateevents.ResolveCorporateEventsRequest(), cancellationToken: token);
        var record = new SettleCorporateEventsMessageRecord
        {
            Message = got.Result.Message,
            Status = got.Result.Status,
            NextPageToken = got.PageInfo.NextPageToken,
            CorrelationId = got.Result.CorrelationId,
        };
        await ActOnSettleCorporateEventsMessageAsync("route-query", record, channel, token);
    }

    private static async Task ActOnSettleCorporateEventsMessageAsync(string kind, SettleCorporateEventsMessageRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["route-query"] = () => new SettleCorporateEventsMessageRouteQueryTarget(new ApiRouting.RouteQueryService.RouteQueryServiceClient(channel)).PushAsync(record, token),
            ["device-policy-validation"] = () => new SettleCorporateEventsMessageDevicePolicyValidationWriter(new ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient(channel)).PushAsync(record, token),
        };
        await actions[kind]();
    }
}
