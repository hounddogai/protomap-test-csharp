using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiAppointments = Acme.Appointments.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiLearning = Acme.Learning.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.WorkplaceBackfill;

/// <summary>Where SettleLearningPathRatesStatus hands each record.</summary>
internal interface ISettleLearningPathRatesStatusSink
{
    Task DeliverAsync(SettleLearningPathRatesStatusRecord record, CancellationToken token);
}

/// <summary>What SettleLearningPathRatesStatus keeps of each GetLearningPathRates response.</summary>
internal sealed class SettleLearningPathRatesStatusRecord
{
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Writes records to acme.appointments.v1.AppointmentService.</summary>
internal sealed class SettleLearningPathRatesStatusAppointmentTarget(ApiAppointments.AppointmentService.AppointmentServiceClient client) : ISettleLearningPathRatesStatusSink
{
    public async Task DeliverAsync(SettleLearningPathRatesStatusRecord record, CancellationToken token)
    {
        await client.UpdateAppointmentAsync(new ApiAppointments.UpdateAppointmentRequest { Appointment = new ApiAppointments.Appointment { Title = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.travel.v1.TravelBookingQueryService.</summary>
internal sealed class SettleLearningPathRatesStatusTravelBookingQueryTarget(ApiTravel.TravelBookingQueryService.TravelBookingQueryServiceClient client) : ISettleLearningPathRatesStatusSink
{
    public async Task DeliverAsync(SettleLearningPathRatesStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTravel.PeekTravelBookingRequest { TravelBookingId = record.Status }, token, client.PeekTravelBookingAsync);
    }
}

/// <summary>Writes records to acme.employeeportal.v1.PortalHomeService.</summary>
internal sealed class SettleLearningPathRatesStatusPortalHomeRelay : ISettleLearningPathRatesStatusSink
{
    private readonly ApiEmployeeportal.PortalHomeService.PortalHomeServiceClient _client;

    public SettleLearningPathRatesStatusPortalHomeRelay(ApiEmployeeportal.PortalHomeService.PortalHomeServiceClient client) => _client = client;

    public async Task DeliverAsync(SettleLearningPathRatesStatusRecord record, CancellationToken token)
    {
        await _client.SearchPortalHomesAsync(new ApiEmployeeportal.SearchPortalHomesRequest { Filter = new ApiEmployeeportal.SearchPortalHomesRequest.Types.Filter { Query = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.learning.v1.LearningPathCalculationService/GetLearningPathRates and hands what it keeps to the sink of its kind.</summary>
internal static class SettleLearningPathRatesStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient(channel);
        var reply = await source.GetLearningPathRatesAsync(new ApiLearning.GetLearningPathRatesRequest() { LearningPathId = "main-settle-learning-path-rates-status" }, cancellationToken: token);
        var record = new SettleLearningPathRatesStatusRecord
        {
            Status = reply.Result.Status,
            CorrelationId = reply.Result.CorrelationId,
        };
        await ActOnSettleLearningPathRatesStatusAsync("travel-booking-query", record, channel, token);
    }

    private static async Task ActOnSettleLearningPathRatesStatusAsync(string kind, SettleLearningPathRatesStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["appointment"] = () => new SettleLearningPathRatesStatusAppointmentTarget(new ApiAppointments.AppointmentService.AppointmentServiceClient(channel)).DeliverAsync(record, token),
            ["travel-booking-query"] = () => new SettleLearningPathRatesStatusTravelBookingQueryTarget(new ApiTravel.TravelBookingQueryService.TravelBookingQueryServiceClient(channel)).DeliverAsync(record, token),
            ["portal-home"] = () => new SettleLearningPathRatesStatusPortalHomeRelay(new ApiEmployeeportal.PortalHomeService.PortalHomeServiceClient(channel)).DeliverAsync(record, token),
        };
        await actions[kind]();
    }
}
