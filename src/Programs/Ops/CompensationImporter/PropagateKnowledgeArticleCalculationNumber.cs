using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecommendations = Acme.Recommendations.V1;
using ApiReturns = Acme.Returns.V1;

namespace Acme.Workforce.Ops.CompensationImporter;

/// <summary>Where PropagateKnowledgeArticleCalculationNumber hands each record.</summary>
internal interface IPropagateKnowledgeArticleCalculationNumberSink
{
    Task PushAsync(PropagateKnowledgeArticleCalculationNumberRecord record, CancellationToken token);
}

/// <summary>What PropagateKnowledgeArticleCalculationNumber keeps of each ExplainKnowledgeArticleCalculation response.</summary>
internal sealed class PropagateKnowledgeArticleCalculationNumberRecord
{
    public string Number { get; init; } = "";
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string ResultMessage { get; init; } = "";
}

/// <summary>Sends records to acme.performance.v1.CalibrationValidationService.</summary>
internal sealed class PropagateKnowledgeArticleCalculationNumberCalibrationValidationSink : IPropagateKnowledgeArticleCalculationNumberSink
{
    private readonly ApiPerformance.CalibrationValidationService.CalibrationValidationServiceClient _client;

    public PropagateKnowledgeArticleCalculationNumberCalibrationValidationSink(ApiPerformance.CalibrationValidationService.CalibrationValidationServiceClient client) => _client = client;

    public async Task PushAsync(PropagateKnowledgeArticleCalculationNumberRecord record, CancellationToken token)
    {
        await _client.ScoreCalibrationAsync(new ApiPerformance.ScoreCalibrationRequest { Calibration = new ApiPerformance.Calibration { Subject = record.Number }, Details = new ApiPerformance.ScoreCalibrationRequest.Types.Details { CurrencyCode = record.ResultMessage } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.recommendations.v1.RecommendationSetQueryService.</summary>
internal sealed class PropagateKnowledgeArticleCalculationNumberRecommendationSetQueryWriter(ApiRecommendations.RecommendationSetQueryService.RecommendationSetQueryServiceClient client) : IPropagateKnowledgeArticleCalculationNumberSink
{
    public async Task PushAsync(PropagateKnowledgeArticleCalculationNumberRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiRecommendations.FindRecommendationSetsRequest { OrderBy = record.Message }, token, client.FindRecommendationSetsAsync);
    }
}

/// <summary>Writes records to acme.returns.v1.ReturnRequestCalculationService.</summary>
internal sealed class PropagateKnowledgeArticleCalculationNumberReturnRequestCalculationSink(ApiReturns.ReturnRequestCalculationService.ReturnRequestCalculationServiceClient client) : IPropagateKnowledgeArticleCalculationNumberSink
{
    public async Task PushAsync(PropagateKnowledgeArticleCalculationNumberRecord record, CancellationToken token)
    {
        await client.GetReturnRequestRatesAsync(new ApiReturns.GetReturnRequestRatesRequest { ReturnRequestId = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Reads acme.helpdesk.v1.KnowledgeArticleCalculationService/ExplainKnowledgeArticleCalculation and hands what it keeps to the sink of its kind.</summary>
internal static class PropagateKnowledgeArticleCalculationNumber
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient(channel);
        var reply = await source.ExplainKnowledgeArticleCalculationAsync(new ApiHelpdesk.ExplainKnowledgeArticleCalculationRequest() { Reason = "current-propagate-knowledge-article-calculation-number" }, cancellationToken: token);
        var record = new PropagateKnowledgeArticleCalculationNumberRecord
        {
            Number = reply.KnowledgeArticle.Number,
            Message = reply.Outcome.Message,
            CorrelationId = reply.Result.CorrelationId,
            ResultMessage = Text.Normalize(reply.Result.Message),
        };
        await ActOnPropagateKnowledgeArticleCalculationNumberAsync("return-request-calculation", record, channel, token);
    }

    private static async Task ActOnPropagateKnowledgeArticleCalculationNumberAsync(string kind, PropagateKnowledgeArticleCalculationNumberRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["calibration-validation"] = () => new PropagateKnowledgeArticleCalculationNumberCalibrationValidationSink(new ApiPerformance.CalibrationValidationService.CalibrationValidationServiceClient(channel)).PushAsync(record, token),
            ["recommendation-set-query"] = () => new PropagateKnowledgeArticleCalculationNumberRecommendationSetQueryWriter(new ApiRecommendations.RecommendationSetQueryService.RecommendationSetQueryServiceClient(channel)).PushAsync(record, token),
            ["return-request-calculation"] = () => new PropagateKnowledgeArticleCalculationNumberReturnRequestCalculationSink(new ApiReturns.ReturnRequestCalculationService.ReturnRequestCalculationServiceClient(channel)).PushAsync(record, token),
        };
        await actions[kind]();
    }
}
