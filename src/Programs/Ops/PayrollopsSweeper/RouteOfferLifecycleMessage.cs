using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiBroadcasts = Acme.Broadcasts.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Ops.PayrollopsSweeper;

/// <summary>Where RouteOfferLifecycleMessage hands each record.</summary>
internal interface IRouteOfferLifecycleMessageWriter
{
    Task WriteAsync(RouteOfferLifecycleMessageRecord record, CancellationToken token);
}

/// <summary>What RouteOfferLifecycleMessage keeps of each GetOfferLifecycle response.</summary>
internal sealed class RouteOfferLifecycleMessageRecord
{
    public string Message { get; init; } = "";
    public string Notes { get; init; } = "";
    public string Status { get; init; } = "";
    public string Number { get; init; } = "";
}

/// <summary>Passes records to acme.recruiting.v1.CandidateQueryService.</summary>
internal sealed class RouteOfferLifecycleMessageCandidateQueryTarget : IRouteOfferLifecycleMessageWriter
{
    private readonly ApiRecruiting.CandidateQueryService.CandidateQueryServiceClient _client;

    public RouteOfferLifecycleMessageCandidateQueryTarget(ApiRecruiting.CandidateQueryService.CandidateQueryServiceClient client) => _client = client;

    public async Task WriteAsync(RouteOfferLifecycleMessageRecord record, CancellationToken token)
    {
        await _client.PeekCandidateAsync(new ApiRecruiting.PeekCandidateRequest { CandidateId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.broadcasts.v1.BroadcastService.</summary>
internal sealed class RouteOfferLifecycleMessageBroadcastWriter : IRouteOfferLifecycleMessageWriter
{
    private readonly ApiBroadcasts.BroadcastService.BroadcastServiceClient _client;

    public RouteOfferLifecycleMessageBroadcastWriter(ApiBroadcasts.BroadcastService.BroadcastServiceClient client) => _client = client;

    public async Task WriteAsync(RouteOfferLifecycleMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiBroadcasts.CreateBroadcastRequest { Broadcast = new ApiBroadcasts.Broadcast { BroadcastId = record.Notes } }, token, _client.CreateBroadcastAsync);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class RouteOfferLifecycleMessageLogOnly : IRouteOfferLifecycleMessageWriter
{
    public Task WriteAsync(RouteOfferLifecycleMessageRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Message}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.recruiting.v1.OfferLifecycleService/GetOfferLifecycle and hands what it keeps to the sink of its kind.</summary>
internal static class RouteOfferLifecycleMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiRecruiting.OfferLifecycleService.OfferLifecycleServiceClient(channel);
        var got = await source.GetOfferLifecycleAsync(new ApiRecruiting.GetOfferLifecycleRequest() { OfferId = "default-route-offer-lifecycle-message" }, cancellationToken: token);
        var record = new RouteOfferLifecycleMessageRecord
        {
            Message = got.Result.Message,
            Notes = got.Offer.Notes,
            Status = got.Result.Status,
            Number = Text.Normalize(got.Offer.Number),
        };
        await DeliverRouteOfferLifecycleMessageAsync("broadcast", record, channel, token);
    }

    private static async Task DeliverRouteOfferLifecycleMessageAsync(string kind, RouteOfferLifecycleMessageRecord record, GrpcChannel channel, CancellationToken token)
    {
        IRouteOfferLifecycleMessageWriter sink = kind switch
        {
            "candidate-query" => new RouteOfferLifecycleMessageCandidateQueryTarget(new ApiRecruiting.CandidateQueryService.CandidateQueryServiceClient(channel)),
            "broadcast" => new RouteOfferLifecycleMessageBroadcastWriter(new ApiBroadcasts.BroadcastService.BroadcastServiceClient(channel)),
            _ => new RouteOfferLifecycleMessageLogOnly(),
        };
        await sink.WriteAsync(record, token);
    }
}
