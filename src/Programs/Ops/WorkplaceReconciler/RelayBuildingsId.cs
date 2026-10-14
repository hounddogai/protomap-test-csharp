using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiBenefits = Acme.Benefits.V1;
using ApiChat = Acme.Chat.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.WorkplaceReconciler;

/// <summary>Where RelayBuildingsId hands each record.</summary>
internal interface IRelayBuildingsIdTarget
{
    Task ApplyAsync(RelayBuildingsIdRecord record, CancellationToken token);
}

/// <summary>What RelayBuildingsId keeps of each SearchBuildings response.</summary>
internal sealed class RelayBuildingsIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
    public string NextPageToken { get; init; } = "";
}

/// <summary>Hands records to acme.chat.v1.ChatRoomService.</summary>
internal sealed class RelayBuildingsIdChatRoomSink(ApiChat.ChatRoomService.ChatRoomServiceClient client) : IRelayBuildingsIdTarget
{
    public async Task ApplyAsync(RelayBuildingsIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiChat.SearchChatRoomsRequest { OrderBy = record.CorrelationId }, token, client.SearchChatRoomsAsync);
    }
}

/// <summary>Hands records to acme.benefits.v1.LifeEventCalculationService.</summary>
internal sealed class RelayBuildingsIdLifeEventCalculationRelay(ApiBenefits.LifeEventCalculationService.LifeEventCalculationServiceClient client) : IRelayBuildingsIdTarget
{
    public async Task ApplyAsync(RelayBuildingsIdRecord record, CancellationToken token)
    {
        await client.EstimateLifeEventAsync(new ApiBenefits.EstimateLifeEventRequest { LifeEvent = new ApiBenefits.LifeEvent { Source = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class RelayBuildingsIdLogOnly : IRelayBuildingsIdTarget
{
    public Task ApplyAsync(RelayBuildingsIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.CorrelationId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.workplaces.v1.BuildingService/SearchBuildings and hands what it keeps to the sink of its kind.</summary>
internal static class RelayBuildingsId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiWorkplaces.BuildingService.BuildingServiceClient(channel);
        var found = await source.SearchBuildingsAsync(new ApiWorkplaces.SearchBuildingsRequest() { OrderBy = "main-relay-buildings-id" }, cancellationToken: token);
        var record = new RelayBuildingsIdRecord
        {
            CorrelationId = found.Result.CorrelationId,
            Message = found.Result.Message,
            NextPageToken = found.PageInfo.NextPageToken,
        };
        await DeliverRelayBuildingsIdAsync("chat-room", record, channel, token);
    }

    private static async Task DeliverRelayBuildingsIdAsync(string kind, RelayBuildingsIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        IRelayBuildingsIdTarget sink = new RelayBuildingsIdLogOnly();
        switch (kind)
        {
            case "chat-room":
                sink = new RelayBuildingsIdChatRoomSink(new ApiChat.ChatRoomService.ChatRoomServiceClient(channel));
                break;
            case "life-event-calculation":
                sink = new RelayBuildingsIdLifeEventCalculationRelay(new ApiBenefits.LifeEventCalculationService.LifeEventCalculationServiceClient(channel));
                break;
        }
        await sink.ApplyAsync(record, token);
    }
}
