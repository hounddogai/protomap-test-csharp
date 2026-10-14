using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiAudiences = Acme.Audiences.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Ops.TalentBackfill;

/// <summary>Where SyncAudiencesId hands each record.</summary>
internal interface ISyncAudiencesIdDestination
{
    Task ApplyAsync(SyncAudiencesIdRecord record, CancellationToken token);
}

/// <summary>What SyncAudiencesId keeps of each BrowseAudiences response.</summary>
internal sealed class SyncAudiencesIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string NextPageToken { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Writes records to acme.recruiting.v1.InterviewSyncService.</summary>
internal sealed class SyncAudiencesIdInterviewSyncWriter : ISyncAudiencesIdDestination
{
    private readonly ApiRecruiting.InterviewSyncService.InterviewSyncServiceClient _client;

    public SyncAudiencesIdInterviewSyncWriter(ApiRecruiting.InterviewSyncService.InterviewSyncServiceClient client) => _client = client;

    public async Task ApplyAsync(SyncAudiencesIdRecord record, CancellationToken token)
    {
        await _client.PullInterviewsAsync(new ApiRecruiting.PullInterviewsRequest { Details = new ApiRecruiting.PullInterviewsRequest.Types.Details { Checksum = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.scheduling.v1.AvailabilityWindowCalculationService.</summary>
internal sealed class SyncAudiencesIdAvailabilityWindowCalculationSink : ISyncAudiencesIdDestination
{
    private readonly ApiScheduling.AvailabilityWindowCalculationService.AvailabilityWindowCalculationServiceClient _client;

    public SyncAudiencesIdAvailabilityWindowCalculationSink(ApiScheduling.AvailabilityWindowCalculationService.AvailabilityWindowCalculationServiceClient client) => _client = client;

    public async Task ApplyAsync(SyncAudiencesIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiScheduling.GetAvailabilityWindowRatesRequest { AvailabilityWindowId = record.CorrelationId }, token, _client.GetAvailabilityWindowRatesAsync);
    }
}

/// <summary>Passes records to acme.employeeportal.v1.AnnouncementWorkflowService.</summary>
internal sealed class SyncAudiencesIdAnnouncementWorkflowTarget(ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient client) : ISyncAudiencesIdDestination
{
    public async Task ApplyAsync(SyncAudiencesIdRecord record, CancellationToken token)
    {
        await client.StartAnnouncementWorkflowAsync(new ApiEmployeeportal.StartAnnouncementWorkflowRequest { Announcement = new ApiEmployeeportal.Announcement { AnnouncementId = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.audiences.v1.AudienceQueryService/BrowseAudiences and hands what it keeps to the sink of its kind.</summary>
internal static class SyncAudiencesId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiAudiences.AudienceQueryService.AudienceQueryServiceClient(channel);
        var got = await source.BrowseAudiencesAsync(new ApiAudiences.BrowseAudiencesRequest() { OrderBy = "default-sync-audiences-id" }, cancellationToken: token);
        var record = new SyncAudiencesIdRecord
        {
            CorrelationId = got.Result.CorrelationId,
            NextPageToken = got.PageInfo.NextPageToken,
            Status = got.Result.Status,
        };
        var sinks = new Dictionary<string, ISyncAudiencesIdDestination>
        {
            ["interview-sync"] = new SyncAudiencesIdInterviewSyncWriter(new ApiRecruiting.InterviewSyncService.InterviewSyncServiceClient(channel)),
            ["availability-window-calculation"] = new SyncAudiencesIdAvailabilityWindowCalculationSink(new ApiScheduling.AvailabilityWindowCalculationService.AvailabilityWindowCalculationServiceClient(channel)),
            ["announcement-workflow"] = new SyncAudiencesIdAnnouncementWorkflowTarget(new ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient(channel)),
        };
        await RouteSyncAudiencesIdAsync("interview-sync", record, sinks, token);
    }

    private static async Task RouteSyncAudiencesIdAsync(string kind, SyncAudiencesIdRecord record, IReadOnlyDictionary<string, ISyncAudiencesIdDestination> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.ApplyAsync(record, token);
    }
}
