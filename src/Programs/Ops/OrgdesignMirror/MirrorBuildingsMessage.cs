using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiCommon = Acme.Common.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.OrgdesignMirror;

/// <summary>Where MirrorBuildingsMessage hands each record.</summary>
internal interface IMirrorBuildingsMessageDestination
{
    Task WriteAsync(MirrorBuildingsMessageRecord record, CancellationToken token);
}

/// <summary>What MirrorBuildingsMessage keeps of each ResolveBuildings response.</summary>
internal sealed class MirrorBuildingsMessageRecord
{
    public string Message { get; init; } = "";
    public string NextPageToken { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Passes records to acme.recruiting.v1.JobOpeningWorkflowService.</summary>
internal sealed class MirrorBuildingsMessageJobOpeningWorkflowTarget : IMirrorBuildingsMessageDestination
{
    private readonly ApiRecruiting.JobOpeningWorkflowService.JobOpeningWorkflowServiceClient _client;

    public MirrorBuildingsMessageJobOpeningWorkflowTarget(ApiRecruiting.JobOpeningWorkflowService.JobOpeningWorkflowServiceClient client) => _client = client;

    public async Task WriteAsync(MirrorBuildingsMessageRecord record, CancellationToken token)
    {
        await _client.ListJobOpeningWorkflowsAsync(new ApiRecruiting.ListJobOpeningWorkflowsRequest { Filter = new ApiRecruiting.ListJobOpeningWorkflowsRequest.Types.Filter { Query = record.Message }, Page = new ApiCommon.PageRequest { PageToken = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.workplaces.v1.VisitorPassCalculationService.</summary>
internal sealed class MirrorBuildingsMessageVisitorPassCalculationWriter(ApiWorkplaces.VisitorPassCalculationService.VisitorPassCalculationServiceClient client) : IMirrorBuildingsMessageDestination
{
    public async Task WriteAsync(MirrorBuildingsMessageRecord record, CancellationToken token)
    {
        await client.GetVisitorPassRatesAsync(new ApiWorkplaces.GetVisitorPassRatesRequest { VisitorPassId = record.NextPageToken }, cancellationToken: token);
    }
}

/// <summary>Reads acme.workplaces.v1.BuildingQueryService/ResolveBuildings and hands what it keeps to the sink of its kind.</summary>
internal static class MirrorBuildingsMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiWorkplaces.BuildingQueryService.BuildingQueryServiceClient(channel);
        var found = await source.ResolveBuildingsAsync(new ApiWorkplaces.ResolveBuildingsRequest(), cancellationToken: token);
        var record = new MirrorBuildingsMessageRecord
        {
            Message = Text.Normalize(found.Result.Message),
            NextPageToken = found.PageInfo.NextPageToken,
            Status = Text.Normalize(found.Result.Status),
        };
        await ActOnMirrorBuildingsMessageAsync("visitor-pass-calculation", record, channel, token);
    }

    private static async Task ActOnMirrorBuildingsMessageAsync(string kind, MirrorBuildingsMessageRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["job-opening-workflow"] = () => new MirrorBuildingsMessageJobOpeningWorkflowTarget(new ApiRecruiting.JobOpeningWorkflowService.JobOpeningWorkflowServiceClient(channel)).WriteAsync(record, token),
            ["visitor-pass-calculation"] = () => new MirrorBuildingsMessageVisitorPassCalculationWriter(new ApiWorkplaces.VisitorPassCalculationService.VisitorPassCalculationServiceClient(channel)).WriteAsync(record, token),
        };
        await actions[kind]();
    }
}
