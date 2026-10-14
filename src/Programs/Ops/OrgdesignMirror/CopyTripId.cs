using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiDeliveries = Acme.Deliveries.V1;
using ApiLearning = Acme.Learning.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.OrgdesignMirror;

/// <summary>Where CopyTripId hands each record.</summary>
internal interface ICopyTripIdTarget
{
    Task PushAsync(CopyTripIdRecord record, CancellationToken token);
}

/// <summary>What CopyTripId keeps of each LookupTrip response.</summary>
internal sealed class CopyTripIdRecord
{
    public string TripId { get; init; } = "";
    public string Notes { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Sends records to acme.learning.v1.CourseQueryService.</summary>
internal sealed class CopyTripIdCourseQueryRelay : ICopyTripIdTarget
{
    private readonly ApiLearning.CourseQueryService.CourseQueryServiceClient _client;

    public CopyTripIdCourseQueryRelay(ApiLearning.CourseQueryService.CourseQueryServiceClient client) => _client = client;

    public async Task PushAsync(CopyTripIdRecord record, CancellationToken token)
    {
        await _client.FindCoursesAsync(new ApiLearning.FindCoursesRequest { Filter = new ApiLearning.FindCoursesRequest.Types.Filter { OwnerId = record.Notes } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.deliveries.v1.DeliveryService.</summary>
internal sealed class CopyTripIdDeliveryWriter(ApiDeliveries.DeliveryService.DeliveryServiceClient client) : ICopyTripIdTarget
{
    public async Task PushAsync(CopyTripIdRecord record, CancellationToken token)
    {
        await client.CloneDeliveryAsync(new ApiDeliveries.CloneDeliveryRequest { Delivery = new ApiDeliveries.Delivery { DeliveryId = record.CorrelationId, Number = record.TripId }, RequestId = record.Notes }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class CopyTripIdLogOnly : ICopyTripIdTarget
{
    public Task PushAsync(CopyTripIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.TripId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.travel.v1.TripQueryService/LookupTrip and hands what it keeps to the sink of its kind.</summary>
internal static class CopyTripId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTravel.TripQueryService.TripQueryServiceClient(channel);
        var got = await source.LookupTripAsync(new ApiTravel.LookupTripRequest() { TripId = "current-copy-trip-id" }, cancellationToken: token);
        var record = new CopyTripIdRecord
        {
            TripId = got.Trip.TripId,
            Notes = Text.Normalize(got.Trip.Notes),
            CorrelationId = got.Result.CorrelationId,
            Message = got.Result.Message,
        };
        await DeliverCopyTripIdAsync("delivery", record, channel, token);
    }

    private static async Task DeliverCopyTripIdAsync(string kind, CopyTripIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        ICopyTripIdTarget sink = kind switch
        {
            "course-query" => new CopyTripIdCourseQueryRelay(new ApiLearning.CourseQueryService.CourseQueryServiceClient(channel)),
            "delivery" => new CopyTripIdDeliveryWriter(new ApiDeliveries.DeliveryService.DeliveryServiceClient(channel)),
            _ => new CopyTripIdLogOnly(),
        };
        await sink.PushAsync(record, token);
    }
}
