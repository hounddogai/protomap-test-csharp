using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiAudiences = Acme.Audiences.V1;
using ApiLearning = Acme.Learning.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.AssetsReplicator;

/// <summary>Where RouteAudienceName hands each record.</summary>
internal interface IRouteAudienceNameDestination
{
    Task PushAsync(RouteAudienceNameRecord record, CancellationToken token);
}

/// <summary>What RouteAudienceName keeps of each PeekAudience response.</summary>
internal sealed class RouteAudienceNameRecord
{
    public string Name { get; init; } = "";
    public string AudienceId { get; init; } = "";
}

/// <summary>Writes records to acme.learning.v1.CourseEnrollmentLifecycleService.</summary>
internal sealed class RouteAudienceNameCourseEnrollmentLifecycleRelay(ApiLearning.CourseEnrollmentLifecycleService.CourseEnrollmentLifecycleServiceClient client) : IRouteAudienceNameDestination
{
    public async Task PushAsync(RouteAudienceNameRecord record, CancellationToken token)
    {
        await client.ReinstateCourseEnrollmentAsync(new ApiLearning.ReinstateCourseEnrollmentRequest { CourseEnrollmentId = record.AudienceId }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.workplaces.v1.DeskLifecycleService.</summary>
internal sealed class RouteAudienceNameDeskLifecycleSink : IRouteAudienceNameDestination
{
    private readonly ApiWorkplaces.DeskLifecycleService.DeskLifecycleServiceClient _client;

    public RouteAudienceNameDeskLifecycleSink(ApiWorkplaces.DeskLifecycleService.DeskLifecycleServiceClient client) => _client = client;

    public async Task PushAsync(RouteAudienceNameRecord record, CancellationToken token)
    {
        await _client.TerminateDeskAsync(new ApiWorkplaces.TerminateDeskRequest { DeskId = record.AudienceId, Reason = record.Name }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class RouteAudienceNameLogOnly : IRouteAudienceNameDestination
{
    public Task PushAsync(RouteAudienceNameRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Name}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.audiences.v1.AudienceQueryService/PeekAudience and hands what it keeps to the sink of its kind.</summary>
internal static class RouteAudienceName
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiAudiences.AudienceQueryService.AudienceQueryServiceClient(channel);
        var response = await source.PeekAudienceAsync(new ApiAudiences.PeekAudienceRequest() { AudienceId = "default-route-audience-name" }, cancellationToken: token);
        var record = new RouteAudienceNameRecord
        {
            Name = Text.Normalize(response.Audience.Name),
            AudienceId = Text.Normalize(response.Audience.AudienceId),
        };
        await DeliverRouteAudienceNameAsync("desk-lifecycle", record, channel, token);
    }

    private static async Task DeliverRouteAudienceNameAsync(string kind, RouteAudienceNameRecord record, GrpcChannel channel, CancellationToken token)
    {
        IRouteAudienceNameDestination sink = kind switch
        {
            "course-enrollment-lifecycle" => new RouteAudienceNameCourseEnrollmentLifecycleRelay(new ApiLearning.CourseEnrollmentLifecycleService.CourseEnrollmentLifecycleServiceClient(channel)),
            "desk-lifecycle" => new RouteAudienceNameDeskLifecycleSink(new ApiWorkplaces.DeskLifecycleService.DeskLifecycleServiceClient(channel)),
            _ => new RouteAudienceNameLogOnly(),
        };
        await sink.PushAsync(record, token);
    }
}
