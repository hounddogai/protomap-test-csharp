using Grpc.Net.Client;
using ApiEmployees = Acme.Employees.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTicketing = Acme.Ticketing.V1;

namespace Acme.Workforce.Ops.PayrollopsSweeper;

/// <summary>Where PushTicketId hands each record.</summary>
internal interface IPushTicketIdDestination
{
    Task DeliverAsync(PushTicketIdRecord record, CancellationToken token);
}

/// <summary>What PushTicketId keeps of each CloneTicket response.</summary>
internal sealed class PushTicketIdRecord
{
    public string TicketId { get; init; } = "";
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Passes records to acme.performance.v1.CalibrationLifecycleService.</summary>
internal sealed class PushTicketIdCalibrationLifecycleRelay : IPushTicketIdDestination
{
    private readonly ApiPerformance.CalibrationLifecycleService.CalibrationLifecycleServiceClient _client;

    public PushTicketIdCalibrationLifecycleRelay(ApiPerformance.CalibrationLifecycleService.CalibrationLifecycleServiceClient client) => _client = client;

    public async Task DeliverAsync(PushTicketIdRecord record, CancellationToken token)
    {
        await _client.ScheduleCalibrationExpiryAsync(new ApiPerformance.ScheduleCalibrationExpiryRequest { Calibration = new ApiPerformance.Calibration { Subject = record.CorrelationId, CalibrationId = record.TicketId }, Details = new ApiPerformance.ScheduleCalibrationExpiryRequest.Types.Details { InitiatedBy = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.scheduling.v1.ShiftQueryService.</summary>
internal sealed class PushTicketIdShiftQuerySink(ApiScheduling.ShiftQueryService.ShiftQueryServiceClient client) : IPushTicketIdDestination
{
    public async Task DeliverAsync(PushTicketIdRecord record, CancellationToken token)
    {
        await client.BrowseShiftsAsync(new ApiScheduling.BrowseShiftsRequest { Filter = new ApiScheduling.BrowseShiftsRequest.Types.Filter { Region = record.CorrelationId, Query = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.itsm.v1.ChangeRequestCalculationService.</summary>
internal sealed class PushTicketIdChangeRequestCalculationSink : IPushTicketIdDestination
{
    private readonly ApiItsm.ChangeRequestCalculationService.ChangeRequestCalculationServiceClient _client;

    public PushTicketIdChangeRequestCalculationSink(ApiItsm.ChangeRequestCalculationService.ChangeRequestCalculationServiceClient client) => _client = client;

    public async Task DeliverAsync(PushTicketIdRecord record, CancellationToken token)
    {
        await _client.SimulateChangeRequestAsync(new ApiItsm.SimulateChangeRequestRequest { Reason = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.employees.v1.EmployeeService.</summary>
internal sealed class PushTicketIdEmployeeRelay : IPushTicketIdDestination
{
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _client;

    public PushTicketIdEmployeeRelay(ApiEmployees.EmployeeService.EmployeeServiceClient client) => _client = client;

    public async Task DeliverAsync(PushTicketIdRecord record, CancellationToken token)
    {
        await _client.GetEmployeeAsync(new ApiEmployees.GetEmployeeRequest { EmployeeId = record.TicketId }, cancellationToken: token);
    }
}

/// <summary>Reads acme.ticketing.v1.TicketService/CloneTicket and hands what it keeps to the sink of its kind.</summary>
internal static class PushTicketId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTicketing.TicketService.TicketServiceClient(channel);
        var response = await source.CloneTicketAsync(new ApiTicketing.CloneTicketRequest() { RequestId = "nightly-push-ticket-id" }, cancellationToken: token);
        var record = new PushTicketIdRecord
        {
            TicketId = response.Ticket.TicketId,
            Message = response.Result.Message,
            CorrelationId = response.Result.CorrelationId,
        };
        var sinks = new Dictionary<string, IPushTicketIdDestination>
        {
            ["calibration-lifecycle"] = new PushTicketIdCalibrationLifecycleRelay(new ApiPerformance.CalibrationLifecycleService.CalibrationLifecycleServiceClient(channel)),
            ["shift-query"] = new PushTicketIdShiftQuerySink(new ApiScheduling.ShiftQueryService.ShiftQueryServiceClient(channel)),
            ["change-request-calculation"] = new PushTicketIdChangeRequestCalculationSink(new ApiItsm.ChangeRequestCalculationService.ChangeRequestCalculationServiceClient(channel)),
            ["employee"] = new PushTicketIdEmployeeRelay(new ApiEmployees.EmployeeService.EmployeeServiceClient(channel)),
        };
        await RoutePushTicketIdAsync("shift-query", record, sinks, token);
    }

    private static async Task RoutePushTicketIdAsync(string kind, PushTicketIdRecord record, IReadOnlyDictionary<string, IPushTicketIdDestination> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.DeliverAsync(record, token);
    }
}
