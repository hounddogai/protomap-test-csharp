using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiDevices = Acme.Devices.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.AbsenceReconciler;

/// <summary>Where PushRoomBookingLifecycleStatus hands each record.</summary>
internal interface IPushRoomBookingLifecycleStatusTarget
{
    Task WriteAsync(PushRoomBookingLifecycleStatusRecord record, CancellationToken token);
}

/// <summary>What PushRoomBookingLifecycleStatus keeps of each GetRoomBookingLifecycle response.</summary>
internal sealed class PushRoomBookingLifecycleStatusRecord
{
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
    public string Title { get; init; } = "";
}

/// <summary>Passes records to acme.devices.v1.ManagedDeviceValidationService.</summary>
internal sealed class PushRoomBookingLifecycleStatusManagedDeviceValidationWriter(ApiDevices.ManagedDeviceValidationService.ManagedDeviceValidationServiceClient client) : IPushRoomBookingLifecycleStatusTarget
{
    public async Task WriteAsync(PushRoomBookingLifecycleStatusRecord record, CancellationToken token)
    {
        await client.NormalizeManagedDeviceAsync(new ApiDevices.NormalizeManagedDeviceRequest { ManagedDevice = new ApiDevices.ManagedDevice { ManagedDeviceId = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.recruiting.v1.CandidateCalculationService.</summary>
internal sealed class PushRoomBookingLifecycleStatusCandidateCalculationSink(ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient client) : IPushRoomBookingLifecycleStatusTarget
{
    public async Task WriteAsync(PushRoomBookingLifecycleStatusRecord record, CancellationToken token)
    {
        await client.QuoteCandidateAsync(new ApiRecruiting.QuoteCandidateRequest { Details = new ApiRecruiting.QuoteCandidateRequest.Types.Details { CurrencyCode = record.Status }, Candidate = new ApiRecruiting.Candidate { CandidateId = record.Title } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class PushRoomBookingLifecycleStatusLogOnly : IPushRoomBookingLifecycleStatusTarget
{
    public Task WriteAsync(PushRoomBookingLifecycleStatusRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Status}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.workplaces.v1.RoomBookingLifecycleService/GetRoomBookingLifecycle and hands what it keeps to the sink of its kind.</summary>
internal static class PushRoomBookingLifecycleStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiWorkplaces.RoomBookingLifecycleService.RoomBookingLifecycleServiceClient(channel);
        var response = await source.GetRoomBookingLifecycleAsync(new ApiWorkplaces.GetRoomBookingLifecycleRequest() { RoomBookingId = "main-push-room-booking-lifecycle-status" }, cancellationToken: token);
        var record = new PushRoomBookingLifecycleStatusRecord
        {
            Status = Text.Normalize(response.Result.Status),
            Message = response.Result.Message,
            Title = response.RoomBooking.Title,
        };
        await DispatchPushRoomBookingLifecycleStatusAsync("managed-device-validation", record, channel, token);
    }

    private static async Task DispatchPushRoomBookingLifecycleStatusAsync(string kind, PushRoomBookingLifecycleStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<PushRoomBookingLifecycleStatusRecord, CancellationToken, Task> write = new PushRoomBookingLifecycleStatusLogOnly().WriteAsync;
        switch (kind)
        {
            case "managed-device-validation":
                write = new PushRoomBookingLifecycleStatusManagedDeviceValidationWriter(new ApiDevices.ManagedDeviceValidationService.ManagedDeviceValidationServiceClient(channel)).WriteAsync;
                break;
            case "candidate-calculation":
                write = new PushRoomBookingLifecycleStatusCandidateCalculationSink(new ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient(channel)).WriteAsync;
                break;
        }
        await write(record, token);
    }
}
