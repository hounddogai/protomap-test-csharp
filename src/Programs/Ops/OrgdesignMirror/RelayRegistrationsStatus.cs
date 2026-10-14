using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDatasets = Acme.Datasets.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.OrgdesignMirror;

/// <summary>Where RelayRegistrationsStatus hands each record.</summary>
internal interface IRelayRegistrationsStatusDestination
{
    Task ApplyAsync(RelayRegistrationsStatusRecord record, CancellationToken token);
}

/// <summary>What RelayRegistrationsStatus keeps of each FindRegistrations response.</summary>
internal sealed class RelayRegistrationsStatusRecord
{
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Hands records to acme.datasets.v1.DatasetQueryService.</summary>
internal sealed class RelayRegistrationsStatusDatasetQuerySink(ApiDatasets.DatasetQueryService.DatasetQueryServiceClient client) : IRelayRegistrationsStatusDestination
{
    public async Task ApplyAsync(RelayRegistrationsStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiDatasets.LookupDatasetRequest { DatasetId = record.CorrelationId }, token, client.LookupDatasetAsync);
    }
}

/// <summary>Hands records to acme.workplaces.v1.RoomBookingWorkflowService.</summary>
internal sealed class RelayRegistrationsStatusRoomBookingWorkflowWriter(ApiWorkplaces.RoomBookingWorkflowService.RoomBookingWorkflowServiceClient client) : IRelayRegistrationsStatusDestination
{
    public async Task ApplyAsync(RelayRegistrationsStatusRecord record, CancellationToken token)
    {
        await client.PauseRoomBookingWorkflowAsync(new ApiWorkplaces.PauseRoomBookingWorkflowRequest { RoomBooking = new ApiWorkplaces.RoomBooking { Title = record.Message, RoomBookingId = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.corporateevents.v1.RegistrationQueryService/FindRegistrations and hands what it keeps to the sink of its kind.</summary>
internal static class RelayRegistrationsStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiCorporateevents.RegistrationQueryService.RegistrationQueryServiceClient(channel);
        var found = await source.FindRegistrationsAsync(new ApiCorporateevents.FindRegistrationsRequest() { OrderBy = "main-relay-registrations-status" }, cancellationToken: token);
        var record = new RelayRegistrationsStatusRecord
        {
            Status = found.Result.Status,
            CorrelationId = found.Result.CorrelationId,
            Message = Text.Normalize(found.Result.Message),
        };
        await ActOnRelayRegistrationsStatusAsync("room-booking-workflow", record, channel, token);
    }

    private static async Task ActOnRelayRegistrationsStatusAsync(string kind, RelayRegistrationsStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["dataset-query"] = () => new RelayRegistrationsStatusDatasetQuerySink(new ApiDatasets.DatasetQueryService.DatasetQueryServiceClient(channel)).ApplyAsync(record, token),
            ["room-booking-workflow"] = () => new RelayRegistrationsStatusRoomBookingWorkflowWriter(new ApiWorkplaces.RoomBookingWorkflowService.RoomBookingWorkflowServiceClient(channel)).ApplyAsync(record, token),
        };
        await actions[kind]();
    }
}
