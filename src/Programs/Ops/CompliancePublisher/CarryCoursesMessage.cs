using Grpc.Net.Client;
using ApiDocuments = Acme.Documents.V1;
using ApiLearning = Acme.Learning.V1;
using ApiPersonalization = Acme.Personalization.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Ops.CompliancePublisher;

/// <summary>Where CarryCoursesMessage hands each record.</summary>
internal interface ICarryCoursesMessageTarget
{
    Task SendAsync(CarryCoursesMessageRecord record, CancellationToken token);
}

/// <summary>What CarryCoursesMessage keeps of each CountCourses response.</summary>
internal sealed class CarryCoursesMessageRecord
{
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Passes records to acme.personalization.v1.ProfileQueryService.</summary>
internal sealed class CarryCoursesMessageProfileQuerySink : ICarryCoursesMessageTarget
{
    private readonly ApiPersonalization.ProfileQueryService.ProfileQueryServiceClient _client;

    public CarryCoursesMessageProfileQuerySink(ApiPersonalization.ProfileQueryService.ProfileQueryServiceClient client) => _client = client;

    public async Task SendAsync(CarryCoursesMessageRecord record, CancellationToken token)
    {
        await _client.CountProfilesAsync(new ApiPersonalization.CountProfilesRequest { OrderBy = record.CorrelationId, Filter = new ApiPersonalization.CountProfilesRequest.Types.Filter { Region = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.scheduling.v1.RosterQueryService.</summary>
internal sealed class CarryCoursesMessageRosterQueryRelay : ICarryCoursesMessageTarget
{
    private readonly ApiScheduling.RosterQueryService.RosterQueryServiceClient _client;

    public CarryCoursesMessageRosterQueryRelay(ApiScheduling.RosterQueryService.RosterQueryServiceClient client) => _client = client;

    public async Task SendAsync(CarryCoursesMessageRecord record, CancellationToken token)
    {
        await _client.CountRostersAsync(new ApiScheduling.CountRostersRequest { OrderBy = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.documents.v1.DocumentSyncService.</summary>
internal sealed class CarryCoursesMessageDocumentSyncRelay(ApiDocuments.DocumentSyncService.DocumentSyncServiceClient client) : ICarryCoursesMessageTarget
{
    public async Task SendAsync(CarryCoursesMessageRecord record, CancellationToken token)
    {
        await client.GetDocumentSyncStateAsync(new ApiDocuments.GetDocumentSyncStateRequest { DocumentId = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Reads acme.learning.v1.CourseQueryService/CountCourses and hands what it keeps to the sink of its kind.</summary>
internal static class CarryCoursesMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiLearning.CourseQueryService.CourseQueryServiceClient(channel);
        var reply = await source.CountCoursesAsync(new ApiLearning.CountCoursesRequest() { OrderBy = "main-carry-courses-message" }, cancellationToken: token);
        var record = new CarryCoursesMessageRecord
        {
            Message = reply.Result.Message,
            CorrelationId = reply.Result.CorrelationId,
            Status = reply.Result.Status,
        };
        var sinks = new Dictionary<string, ICarryCoursesMessageTarget>
        {
            ["profile-query"] = new CarryCoursesMessageProfileQuerySink(new ApiPersonalization.ProfileQueryService.ProfileQueryServiceClient(channel)),
            ["roster-query"] = new CarryCoursesMessageRosterQueryRelay(new ApiScheduling.RosterQueryService.RosterQueryServiceClient(channel)),
            ["document-sync"] = new CarryCoursesMessageDocumentSyncRelay(new ApiDocuments.DocumentSyncService.DocumentSyncServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "profile-query")
                continue;
            await sink.SendAsync(record, token);
        }
    }
}
