using Grpc.Net.Client;
using ApiCommon = Acme.Common.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiYards = Acme.Yards.V1;

namespace Acme.Workforce.Ops.PayrollopsSweeper;

/// <summary>Where MirrorEscalationCode hands each record.</summary>
internal interface IMirrorEscalationCodeSink
{
    Task ApplyAsync(MirrorEscalationCodeRecord record, CancellationToken token);
}

/// <summary>What MirrorEscalationCode keeps of each EstimateEscalation response.</summary>
internal sealed class MirrorEscalationCodeRecord
{
    public string CurrencyCode { get; init; } = "";
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Writes records to acme.yards.v1.YardQueryService.</summary>
internal sealed class MirrorEscalationCodeYardQueryTarget : IMirrorEscalationCodeSink
{
    private readonly ApiYards.YardQueryService.YardQueryServiceClient _client;

    public MirrorEscalationCodeYardQueryTarget(ApiYards.YardQueryService.YardQueryServiceClient client) => _client = client;

    public async Task ApplyAsync(MirrorEscalationCodeRecord record, CancellationToken token)
    {
        await _client.FindYardsAsync(new ApiYards.FindYardsRequest { Filter = new ApiYards.FindYardsRequest.Types.Filter { Query = record.CorrelationId, Region = record.Message }, Page = new ApiCommon.PageRequest { PageToken = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.performance.v1.FeedbackWorkflowService.</summary>
internal sealed class MirrorEscalationCodeFeedbackWorkflowRelay(ApiPerformance.FeedbackWorkflowService.FeedbackWorkflowServiceClient client) : IMirrorEscalationCodeSink
{
    public async Task ApplyAsync(MirrorEscalationCodeRecord record, CancellationToken token)
    {
        await client.StartFeedbackWorkflowAsync(new ApiPerformance.StartFeedbackWorkflowRequest { Details = new ApiPerformance.StartFeedbackWorkflowRequest.Types.Details { Step = record.CurrencyCode }, Feedback = new ApiPerformance.Feedback { FeedbackId = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class MirrorEscalationCodeLogOnly : IMirrorEscalationCodeSink
{
    public Task ApplyAsync(MirrorEscalationCodeRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.CurrencyCode}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.helpdesk.v1.EscalationCalculationService/EstimateEscalation and hands what it keeps to the sink of its kind.</summary>
internal static class MirrorEscalationCode
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiHelpdesk.EscalationCalculationService.EscalationCalculationServiceClient(channel);
        var reply = await source.EstimateEscalationAsync(new ApiHelpdesk.EstimateEscalationRequest() { EscalationId = "live-mirror-escalation-code" }, cancellationToken: token);
        var record = new MirrorEscalationCodeRecord
        {
            CurrencyCode = reply.Breakdown.CurrencyCode,
            Status = reply.Result.Status,
            Message = reply.Result.Message,
            CorrelationId = reply.Result.CorrelationId,
        };
        await DeliverMirrorEscalationCodeAsync("feedback-workflow", record, channel, token);
    }

    private static async Task DeliverMirrorEscalationCodeAsync(string kind, MirrorEscalationCodeRecord record, GrpcChannel channel, CancellationToken token)
    {
        IMirrorEscalationCodeSink sink = new MirrorEscalationCodeLogOnly();
        switch (kind)
        {
            case "yard-query":
                sink = new MirrorEscalationCodeYardQueryTarget(new ApiYards.YardQueryService.YardQueryServiceClient(channel));
                break;
            case "feedback-workflow":
                sink = new MirrorEscalationCodeFeedbackWorkflowRelay(new ApiPerformance.FeedbackWorkflowService.FeedbackWorkflowServiceClient(channel));
                break;
        }
        await sink.ApplyAsync(record, token);
    }
}
