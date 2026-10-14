using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiAppointments = Acme.Appointments.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiLabeling = Acme.Labeling.V1;

namespace Acme.Workforce.Ops.TalentBackfill;

/// <summary>Where HandOffAppointmentStatus hands each record.</summary>
internal interface IHandOffAppointmentStatusTarget
{
    Task PushAsync(HandOffAppointmentStatusRecord record, CancellationToken token);
}

/// <summary>What HandOffAppointmentStatus keeps of each UpdateAppointment response.</summary>
internal sealed class HandOffAppointmentStatusRecord
{
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Sends records to acme.labeling.v1.LabelingTaskService.</summary>
internal sealed class HandOffAppointmentStatusLabelingTaskRelay : IHandOffAppointmentStatusTarget
{
    private readonly ApiLabeling.LabelingTaskService.LabelingTaskServiceClient _client;

    public HandOffAppointmentStatusLabelingTaskRelay(ApiLabeling.LabelingTaskService.LabelingTaskServiceClient client) => _client = client;

    public async Task PushAsync(HandOffAppointmentStatusRecord record, CancellationToken token)
    {
        await _client.UpdateLabelingTaskAsync(new ApiLabeling.UpdateLabelingTaskRequest { LabelingTask = new ApiLabeling.LabelingTask { LabelingTaskId = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.corporateevents.v1.CorporateEventReportingService.</summary>
internal sealed class HandOffAppointmentStatusCorporateEventReportingRelay(ApiCorporateevents.CorporateEventReportingService.CorporateEventReportingServiceClient client) : IHandOffAppointmentStatusTarget
{
    public async Task PushAsync(HandOffAppointmentStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiCorporateevents.DeleteCorporateEventReportRequest { Etag = record.Message, Reason = record.Status }, token, client.DeleteCorporateEventReportAsync);
    }
}

/// <summary>Reads acme.appointments.v1.AppointmentService/UpdateAppointment and hands what it keeps to the sink of its kind.</summary>
internal static class HandOffAppointmentStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiAppointments.AppointmentService.AppointmentServiceClient(channel);
        var response = await source.UpdateAppointmentAsync(new ApiAppointments.UpdateAppointmentRequest(), cancellationToken: token);
        var record = new HandOffAppointmentStatusRecord
        {
            Status = response.Result.Status,
            Message = response.Result.Message,
        };
        var sinks = new IHandOffAppointmentStatusTarget[]
        {
            new HandOffAppointmentStatusLabelingTaskRelay(new ApiLabeling.LabelingTaskService.LabelingTaskServiceClient(channel)),
            new HandOffAppointmentStatusCorporateEventReportingRelay(new ApiCorporateevents.CorporateEventReportingService.CorporateEventReportingServiceClient(channel)),
        };
        await sinks[1].PushAsync(record, token);
    }
}
