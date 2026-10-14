using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.WorkplaceBackfill;

/// <summary>Where ForwardChangeRequestSubject hands each record.</summary>
internal interface IForwardChangeRequestSubjectDestination
{
    Task PushAsync(ForwardChangeRequestSubjectRecord record, CancellationToken token);
}

/// <summary>What ForwardChangeRequestSubject keeps of each TerminateChangeRequest response.</summary>
internal sealed class ForwardChangeRequestSubjectRecord
{
    public string Subject { get; init; } = "";
    public string Message { get; init; } = "";
    public string ChangeRequestId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Writes records to acme.travel.v1.TravelBookingLifecycleService.</summary>
internal sealed class ForwardChangeRequestSubjectTravelBookingLifecycleTarget : IForwardChangeRequestSubjectDestination
{
    private readonly ApiTravel.TravelBookingLifecycleService.TravelBookingLifecycleServiceClient _client;

    public ForwardChangeRequestSubjectTravelBookingLifecycleTarget(ApiTravel.TravelBookingLifecycleService.TravelBookingLifecycleServiceClient client) => _client = client;

    public async Task PushAsync(ForwardChangeRequestSubjectRecord record, CancellationToken token)
    {
        await _client.ReinstateTravelBookingAsync(new ApiTravel.ReinstateTravelBookingRequest { Reason = record.Status, TravelBookingId = record.ChangeRequestId }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.employeeportal.v1.PortalHomeNotificationService.</summary>
internal sealed class ForwardChangeRequestSubjectPortalHomeNotificationWriter : IForwardChangeRequestSubjectDestination
{
    private readonly ApiEmployeeportal.PortalHomeNotificationService.PortalHomeNotificationServiceClient _client;

    public ForwardChangeRequestSubjectPortalHomeNotificationWriter(ApiEmployeeportal.PortalHomeNotificationService.PortalHomeNotificationServiceClient client) => _client = client;

    public async Task PushAsync(ForwardChangeRequestSubjectRecord record, CancellationToken token)
    {
        await _client.SendPortalHomeTestNoticeAsync(new ApiEmployeeportal.SendPortalHomeTestNoticeRequest { Reason = record.Subject, PortalHomeId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Reads acme.itsm.v1.ChangeRequestLifecycleService/TerminateChangeRequest and hands what it keeps to the sink of its kind.</summary>
internal static class ForwardChangeRequestSubject
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiItsm.ChangeRequestLifecycleService.ChangeRequestLifecycleServiceClient(channel);
        var response = await source.TerminateChangeRequestAsync(new ApiItsm.TerminateChangeRequestRequest() { Reason = "primary-forward-change-request-subject" }, cancellationToken: token);
        var record = new ForwardChangeRequestSubjectRecord
        {
            Subject = response.ChangeRequest.Subject,
            Message = Text.Normalize(response.Result.Message),
            ChangeRequestId = response.ChangeRequest.ChangeRequestId,
            Status = response.Result.Status,
        };
        var sinks = new List<IForwardChangeRequestSubjectDestination>
        {
            new ForwardChangeRequestSubjectTravelBookingLifecycleTarget(new ApiTravel.TravelBookingLifecycleService.TravelBookingLifecycleServiceClient(channel)),
            new ForwardChangeRequestSubjectPortalHomeNotificationWriter(new ApiEmployeeportal.PortalHomeNotificationService.PortalHomeNotificationServiceClient(channel)),
        };
        foreach (var sink in sinks)
            await sink.PushAsync(record, token);
    }
}
