using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCareplans = Acme.Careplans.V1;
using ApiContent = Acme.Content.V1;
using ApiLabeling = Acme.Labeling.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.TalentBackfill;

/// <summary>Where PropagateCarePlanId hands each record.</summary>
internal interface IPropagateCarePlanIdTarget
{
    Task ApplyAsync(PropagateCarePlanIdRecord record, CancellationToken token);
}

/// <summary>What PropagateCarePlanId keeps of each DeleteCarePlan response.</summary>
internal sealed class PropagateCarePlanIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Writes records to acme.content.v1.ArticleService.</summary>
internal sealed class PropagateCarePlanIdArticleWriter : IPropagateCarePlanIdTarget
{
    private readonly ApiContent.ArticleService.ArticleServiceClient _client;

    public PropagateCarePlanIdArticleWriter(ApiContent.ArticleService.ArticleServiceClient client) => _client = client;

    public async Task ApplyAsync(PropagateCarePlanIdRecord record, CancellationToken token)
    {
        await _client.CreateArticleAsync(new ApiContent.CreateArticleRequest { Article = new ApiContent.Article { ArticleId = record.CorrelationId }, RequestId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.workplaces.v1.VisitorPassReportingService.</summary>
internal sealed class PropagateCarePlanIdVisitorPassReportingRelay : IPropagateCarePlanIdTarget
{
    private readonly ApiWorkplaces.VisitorPassReportingService.VisitorPassReportingServiceClient _client;

    public PropagateCarePlanIdVisitorPassReportingRelay(ApiWorkplaces.VisitorPassReportingService.VisitorPassReportingServiceClient client) => _client = client;

    public async Task ApplyAsync(PropagateCarePlanIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiWorkplaces.DeleteVisitorPassReportRequest { VisitorPassId = record.Message, Etag = record.CorrelationId }, token, _client.DeleteVisitorPassReportAsync);
    }
}

/// <summary>Writes records to acme.labeling.v1.LabelingTaskCalculationService.</summary>
internal sealed class PropagateCarePlanIdLabelingTaskCalculationRelay : IPropagateCarePlanIdTarget
{
    private readonly ApiLabeling.LabelingTaskCalculationService.LabelingTaskCalculationServiceClient _client;

    public PropagateCarePlanIdLabelingTaskCalculationRelay(ApiLabeling.LabelingTaskCalculationService.LabelingTaskCalculationServiceClient client) => _client = client;

    public async Task ApplyAsync(PropagateCarePlanIdRecord record, CancellationToken token)
    {
        await _client.EstimateLabelingTaskAsync(new ApiLabeling.EstimateLabelingTaskRequest { Details = new ApiLabeling.EstimateLabelingTaskRequest.Types.Details { CurrencyCode = record.CorrelationId }, Reason = record.Message }, cancellationToken: token);
    }
}

/// <summary>Reads acme.careplans.v1.CarePlanService/DeleteCarePlan and hands what it keeps to the sink of its kind.</summary>
internal static class PropagateCarePlanId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiCareplans.CarePlanService.CarePlanServiceClient(channel);
        var got = await source.DeleteCarePlanAsync(new ApiCareplans.DeleteCarePlanRequest() { CarePlanId = "live-propagate-care-plan-id" }, cancellationToken: token);
        var record = new PropagateCarePlanIdRecord
        {
            CorrelationId = got.Result.CorrelationId,
            Message = got.Result.Message,
        };
        await ActOnPropagateCarePlanIdAsync("labeling-task-calculation", record, channel, token);
    }

    private static async Task ActOnPropagateCarePlanIdAsync(string kind, PropagateCarePlanIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["article"] = () => new PropagateCarePlanIdArticleWriter(new ApiContent.ArticleService.ArticleServiceClient(channel)).ApplyAsync(record, token),
            ["visitor-pass-reporting"] = () => new PropagateCarePlanIdVisitorPassReportingRelay(new ApiWorkplaces.VisitorPassReportingService.VisitorPassReportingServiceClient(channel)).ApplyAsync(record, token),
            ["labeling-task-calculation"] = () => new PropagateCarePlanIdLabelingTaskCalculationRelay(new ApiLabeling.LabelingTaskCalculationService.LabelingTaskCalculationServiceClient(channel)).ApplyAsync(record, token),
        };
        await actions[kind]();
    }
}
