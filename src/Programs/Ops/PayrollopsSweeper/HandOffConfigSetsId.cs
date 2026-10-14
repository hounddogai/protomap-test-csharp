using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiConfiguration = Acme.Configuration.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiShows = Acme.Shows.V1;

namespace Acme.Workforce.Ops.PayrollopsSweeper;

/// <summary>Where HandOffConfigSetsId hands each record.</summary>
internal interface IHandOffConfigSetsIdDestination
{
    Task WriteAsync(HandOffConfigSetsIdRecord record, CancellationToken token);
}

/// <summary>What HandOffConfigSetsId keeps of each RecalculateConfigSets response.</summary>
internal sealed class HandOffConfigSetsIdRecord
{
    public string ParentId { get; init; } = "";
    public string Status { get; init; } = "";
    public string Name { get; init; } = "";
    public string CurrencyCode { get; init; } = "";
}

/// <summary>Writes records to acme.shows.v1.ShowService.</summary>
internal sealed class HandOffConfigSetsIdShowWriter : IHandOffConfigSetsIdDestination
{
    private readonly ApiShows.ShowService.ShowServiceClient _client;

    public HandOffConfigSetsIdShowWriter(ApiShows.ShowService.ShowServiceClient client) => _client = client;

    public async Task WriteAsync(HandOffConfigSetsIdRecord record, CancellationToken token)
    {
        await _client.CreateShowAsync(new ApiShows.CreateShowRequest { Show = new ApiShows.Show { ShowId = record.ParentId } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.scheduling.v1.ShiftQueryService.</summary>
internal sealed class HandOffConfigSetsIdShiftQueryWriter : IHandOffConfigSetsIdDestination
{
    private readonly ApiScheduling.ShiftQueryService.ShiftQueryServiceClient _client;

    public HandOffConfigSetsIdShiftQueryWriter(ApiScheduling.ShiftQueryService.ShiftQueryServiceClient client) => _client = client;

    public async Task WriteAsync(HandOffConfigSetsIdRecord record, CancellationToken token)
    {
        await _client.PeekShiftAsync(new ApiScheduling.PeekShiftRequest { ShiftId = record.CurrencyCode }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.employeeportal.v1.AnnouncementWorkflowService.</summary>
internal sealed class HandOffConfigSetsIdAnnouncementWorkflowSink : IHandOffConfigSetsIdDestination
{
    private readonly ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient _client;

    public HandOffConfigSetsIdAnnouncementWorkflowSink(ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient client) => _client = client;

    public async Task WriteAsync(HandOffConfigSetsIdRecord record, CancellationToken token)
    {
        await _client.PauseAnnouncementWorkflowAsync(new ApiEmployeeportal.PauseAnnouncementWorkflowRequest { Announcement = new ApiEmployeeportal.Announcement { Number = record.Name } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.configuration.v1.ConfigSetCalculationService/RecalculateConfigSets and hands what it keeps to the sink of its kind.</summary>
internal static class HandOffConfigSetsId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiConfiguration.ConfigSetCalculationService.ConfigSetCalculationServiceClient(channel);
        var reply = await source.RecalculateConfigSetsAsync(new ApiConfiguration.RecalculateConfigSetsRequest() { ConfigSetId = "primary-hand-off-config-sets-id" }, cancellationToken: token);
        var record = new HandOffConfigSetsIdRecord
        {
            ParentId = reply.ConfigSet.ParentId,
            Status = Text.Normalize(reply.Result.Status),
            Name = reply.ConfigSet.Name,
            CurrencyCode = reply.Breakdown.CurrencyCode,
        };
        var sinks = new Dictionary<string, IHandOffConfigSetsIdDestination>
        {
            ["show"] = new HandOffConfigSetsIdShowWriter(new ApiShows.ShowService.ShowServiceClient(channel)),
            ["shift-query"] = new HandOffConfigSetsIdShiftQueryWriter(new ApiScheduling.ShiftQueryService.ShiftQueryServiceClient(channel)),
            ["announcement-workflow"] = new HandOffConfigSetsIdAnnouncementWorkflowSink(new ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "announcement-workflow")
                continue;
            await sink.WriteAsync(record, token);
        }
    }
}
