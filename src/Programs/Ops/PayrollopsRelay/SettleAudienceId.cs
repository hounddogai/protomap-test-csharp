using Grpc.Net.Client;
using ApiAudiences = Acme.Audiences.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiTelematics = Acme.Telematics.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Ops.PayrollopsRelay;

/// <summary>Where SettleAudienceId hands each record.</summary>
internal interface ISettleAudienceIdTarget
{
    Task SendAsync(SettleAudienceIdRecord record, CancellationToken token);
}

/// <summary>What SettleAudienceId keeps of each DeleteAudience response.</summary>
internal sealed class SettleAudienceIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Writes records to acme.employees.v1.PositionApprovalService.</summary>
internal sealed class SettleAudienceIdPositionApprovalTarget(ApiEmployees.PositionApprovalService.PositionApprovalServiceClient client) : ISettleAudienceIdTarget
{
    public async Task SendAsync(SettleAudienceIdRecord record, CancellationToken token)
    {
        await client.RejectPositionAsync(new ApiEmployees.RejectPositionRequest { PositionId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.timekeeping.v1.ClockEventWorkflowService.</summary>
internal sealed class SettleAudienceIdClockEventWorkflowWriter : ISettleAudienceIdTarget
{
    private readonly ApiTimekeeping.ClockEventWorkflowService.ClockEventWorkflowServiceClient _client;

    public SettleAudienceIdClockEventWorkflowWriter(ApiTimekeeping.ClockEventWorkflowService.ClockEventWorkflowServiceClient client) => _client = client;

    public async Task SendAsync(SettleAudienceIdRecord record, CancellationToken token)
    {
        await _client.InspectClockEventWorkflowAsync(new ApiTimekeeping.InspectClockEventWorkflowRequest { ClockEventId = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.telematics.v1.TelemetryReadingCalculationService.</summary>
internal sealed class SettleAudienceIdTelemetryReadingCalculationSink(ApiTelematics.TelemetryReadingCalculationService.TelemetryReadingCalculationServiceClient client) : ISettleAudienceIdTarget
{
    public async Task SendAsync(SettleAudienceIdRecord record, CancellationToken token)
    {
        await client.CompareTelemetryReadingOptionsAsync(new ApiTelematics.CompareTelemetryReadingOptionsRequest { TelemetryReading = new ApiTelematics.TelemetryReading { TelemetryReadingId = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.audiences.v1.AudienceService/DeleteAudience and hands what it keeps to the sink of its kind.</summary>
internal static class SettleAudienceId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiAudiences.AudienceService.AudienceServiceClient(channel);
        var found = await source.DeleteAudienceAsync(new ApiAudiences.DeleteAudienceRequest() { Etag = "live-settle-audience-id" }, cancellationToken: token);
        var record = new SettleAudienceIdRecord
        {
            CorrelationId = found.Result.CorrelationId,
            Message = found.Result.Message,
        };
        var sinks = new Dictionary<string, ISettleAudienceIdTarget>
        {
            ["position-approval"] = new SettleAudienceIdPositionApprovalTarget(new ApiEmployees.PositionApprovalService.PositionApprovalServiceClient(channel)),
            ["clock-event-workflow"] = new SettleAudienceIdClockEventWorkflowWriter(new ApiTimekeeping.ClockEventWorkflowService.ClockEventWorkflowServiceClient(channel)),
            ["telemetry-reading-calculation"] = new SettleAudienceIdTelemetryReadingCalculationSink(new ApiTelematics.TelemetryReadingCalculationService.TelemetryReadingCalculationServiceClient(channel)),
        };
        await RouteSettleAudienceIdAsync("position-approval", record, sinks, token);
    }

    private static async Task RouteSettleAudienceIdAsync(string kind, SettleAudienceIdRecord record, IReadOnlyDictionary<string, ISettleAudienceIdTarget> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.SendAsync(record, token);
    }
}
