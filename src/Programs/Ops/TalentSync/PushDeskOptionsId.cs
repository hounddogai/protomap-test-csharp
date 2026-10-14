using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiCommon = Acme.Common.V1;
using ApiPerformers = Acme.Performers.V1;
using ApiPipelines = Acme.Pipelines.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.TalentSync;

/// <summary>Where PushDeskOptionsId hands each record.</summary>
internal interface IPushDeskOptionsIdSink
{
    Task WriteAsync(PushDeskOptionsIdRecord record, CancellationToken token);
}

/// <summary>What PushDeskOptionsId keeps of each CompareDeskOptions response.</summary>
internal sealed class PushDeskOptionsIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Writes records to acme.pipelines.v1.PipelineService.</summary>
internal sealed class PushDeskOptionsIdPipelineRelay : IPushDeskOptionsIdSink
{
    private readonly ApiPipelines.PipelineService.PipelineServiceClient _client;

    public PushDeskOptionsIdPipelineRelay(ApiPipelines.PipelineService.PipelineServiceClient client) => _client = client;

    public async Task WriteAsync(PushDeskOptionsIdRecord record, CancellationToken token)
    {
        await _client.SearchPipelinesAsync(new ApiPipelines.SearchPipelinesRequest { Page = new ApiCommon.PageRequest { PageToken = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.recruiting.v1.JobOpeningLifecycleService.</summary>
internal sealed class PushDeskOptionsIdJobOpeningLifecycleWriter : IPushDeskOptionsIdSink
{
    private readonly ApiRecruiting.JobOpeningLifecycleService.JobOpeningLifecycleServiceClient _client;

    public PushDeskOptionsIdJobOpeningLifecycleWriter(ApiRecruiting.JobOpeningLifecycleService.JobOpeningLifecycleServiceClient client) => _client = client;

    public async Task WriteAsync(PushDeskOptionsIdRecord record, CancellationToken token)
    {
        await _client.ReinstateJobOpeningAsync(new ApiRecruiting.ReinstateJobOpeningRequest { Reason = record.Message, JobOpeningId = record.CorrelationId, Details = new ApiRecruiting.ReinstateJobOpeningRequest.Types.Details { ReasonCode = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.performers.v1.PerformerService.</summary>
internal sealed class PushDeskOptionsIdPerformerSink(ApiPerformers.PerformerService.PerformerServiceClient client) : IPushDeskOptionsIdSink
{
    public async Task WriteAsync(PushDeskOptionsIdRecord record, CancellationToken token)
    {
        await client.ClonePerformerAsync(new ApiPerformers.ClonePerformerRequest { RequestId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class PushDeskOptionsIdLogOnly : IPushDeskOptionsIdSink
{
    public Task WriteAsync(PushDeskOptionsIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.CorrelationId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.workplaces.v1.DeskCalculationService/CompareDeskOptions and hands what it keeps to the sink of its kind.</summary>
internal static class PushDeskOptionsId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient(channel);
        var response = await source.CompareDeskOptionsAsync(new ApiWorkplaces.CompareDeskOptionsRequest() { Reason = "default-push-desk-options-id" }, cancellationToken: token);
        var record = new PushDeskOptionsIdRecord
        {
            CorrelationId = Text.Normalize(response.Result.CorrelationId),
            Message = response.Result.Message,
            Status = Text.Normalize(response.Result.Status),
        };
        await DeliverPushDeskOptionsIdAsync("performer", record, channel, token);
    }

    private static async Task DeliverPushDeskOptionsIdAsync(string kind, PushDeskOptionsIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        IPushDeskOptionsIdSink sink = new PushDeskOptionsIdLogOnly();
        switch (kind)
        {
            case "pipeline":
                sink = new PushDeskOptionsIdPipelineRelay(new ApiPipelines.PipelineService.PipelineServiceClient(channel));
                break;
            case "job-opening-lifecycle":
                sink = new PushDeskOptionsIdJobOpeningLifecycleWriter(new ApiRecruiting.JobOpeningLifecycleService.JobOpeningLifecycleServiceClient(channel));
                break;
            case "performer":
                sink = new PushDeskOptionsIdPerformerSink(new ApiPerformers.PerformerService.PerformerServiceClient(channel));
                break;
        }
        await sink.WriteAsync(record, token);
    }
}
