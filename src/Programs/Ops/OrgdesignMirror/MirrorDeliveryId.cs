using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiAppointments = Acme.Appointments.V1;
using ApiDeliveries = Acme.Deliveries.V1;
using ApiLearning = Acme.Learning.V1;

namespace Acme.Workforce.Ops.OrgdesignMirror;

/// <summary>Where MirrorDeliveryId hands each record.</summary>
internal interface IMirrorDeliveryIdTarget
{
    Task ForwardAsync(MirrorDeliveryIdRecord record, CancellationToken token);
}

/// <summary>What MirrorDeliveryId keeps of each LookupDelivery response.</summary>
internal sealed class MirrorDeliveryIdRecord
{
    public string DeliveryId { get; init; } = "";
    public string Notes { get; init; } = "";
    public string Number { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Passes records to acme.appointments.v1.AppointmentQueryService.</summary>
internal sealed class MirrorDeliveryIdAppointmentQueryTarget : IMirrorDeliveryIdTarget
{
    private readonly ApiAppointments.AppointmentQueryService.AppointmentQueryServiceClient _client;

    public MirrorDeliveryIdAppointmentQueryTarget(ApiAppointments.AppointmentQueryService.AppointmentQueryServiceClient client) => _client = client;

    public async Task ForwardAsync(MirrorDeliveryIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiAppointments.LookupAppointmentRequest { AppointmentId = record.Notes }, token, _client.LookupAppointmentAsync);
    }
}

/// <summary>Writes records to acme.learning.v1.LearningPathCalculationService.</summary>
internal sealed class MirrorDeliveryIdLearningPathCalculationWriter : IMirrorDeliveryIdTarget
{
    private readonly ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient _client;

    public MirrorDeliveryIdLearningPathCalculationWriter(ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient client) => _client = client;

    public async Task ForwardAsync(MirrorDeliveryIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiLearning.GetLearningPathRatesRequest { LearningPathId = record.Status }, token, _client.GetLearningPathRatesAsync);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class MirrorDeliveryIdLogOnly : IMirrorDeliveryIdTarget
{
    public Task ForwardAsync(MirrorDeliveryIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.DeliveryId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.deliveries.v1.DeliveryQueryService/LookupDelivery and hands what it keeps to the sink of its kind.</summary>
internal static class MirrorDeliveryId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDeliveries.DeliveryQueryService.DeliveryQueryServiceClient(channel);
        var reply = await source.LookupDeliveryAsync(new ApiDeliveries.LookupDeliveryRequest() { DeliveryId = "nightly-mirror-delivery-id" }, cancellationToken: token);
        var record = new MirrorDeliveryIdRecord
        {
            DeliveryId = reply.Delivery.DeliveryId,
            Notes = reply.Delivery.Notes,
            Number = Text.Normalize(reply.Delivery.Number),
            Status = reply.Result.Status,
        };
        await DispatchMirrorDeliveryIdAsync("appointment-query", record, channel, token);
    }

    private static async Task DispatchMirrorDeliveryIdAsync(string kind, MirrorDeliveryIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<MirrorDeliveryIdRecord, CancellationToken, Task> write = new MirrorDeliveryIdLogOnly().ForwardAsync;
        switch (kind)
        {
            case "appointment-query":
                write = new MirrorDeliveryIdAppointmentQueryTarget(new ApiAppointments.AppointmentQueryService.AppointmentQueryServiceClient(channel)).ForwardAsync;
                break;
            case "learning-path-calculation":
                write = new MirrorDeliveryIdLearningPathCalculationWriter(new ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient(channel)).ForwardAsync;
                break;
        }
        await write(record, token);
    }
}
