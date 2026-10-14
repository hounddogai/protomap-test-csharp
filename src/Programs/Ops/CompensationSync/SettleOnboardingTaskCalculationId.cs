using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiCommon = Acme.Common.V1;
using ApiCustomers = Acme.Customers.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiRecommendations = Acme.Recommendations.V1;

namespace Acme.Workforce.Ops.CompensationSync;

/// <summary>Where SettleOnboardingTaskCalculationId hands each record.</summary>
internal interface ISettleOnboardingTaskCalculationIdWriter
{
    Task DeliverAsync(SettleOnboardingTaskCalculationIdRecord record, CancellationToken token);
}

/// <summary>What SettleOnboardingTaskCalculationId keeps of each ExplainOnboardingTaskCalculation response.</summary>
internal sealed class SettleOnboardingTaskCalculationIdRecord
{
    public string ReferenceId { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string OnboardingTaskId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Writes records to acme.customers.v1.CustomerService.</summary>
internal sealed class SettleOnboardingTaskCalculationIdCustomerWriter(ApiCustomers.CustomerService.CustomerServiceClient client) : ISettleOnboardingTaskCalculationIdWriter
{
    public async Task DeliverAsync(SettleOnboardingTaskCalculationIdRecord record, CancellationToken token)
    {
        await client.CloneCustomerAsync(new ApiCustomers.CloneCustomerRequest { Customer = new ApiCustomers.Customer { CustomerId = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.recommendations.v1.RecommendationSetQueryService.</summary>
internal sealed class SettleOnboardingTaskCalculationIdRecommendationSetQuerySink : ISettleOnboardingTaskCalculationIdWriter
{
    private readonly ApiRecommendations.RecommendationSetQueryService.RecommendationSetQueryServiceClient _client;

    public SettleOnboardingTaskCalculationIdRecommendationSetQuerySink(ApiRecommendations.RecommendationSetQueryService.RecommendationSetQueryServiceClient client) => _client = client;

    public async Task DeliverAsync(SettleOnboardingTaskCalculationIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiRecommendations.SuggestRecommendationSetsRequest { Filter = new ApiRecommendations.SuggestRecommendationSetsRequest.Types.Filter { OwnerId = record.ReferenceId, Region = record.OnboardingTaskId }, Page = new ApiCommon.PageRequest { PageToken = record.CorrelationId } }, token, _client.SuggestRecommendationSetsAsync);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class SettleOnboardingTaskCalculationIdLogOnly : ISettleOnboardingTaskCalculationIdWriter
{
    public Task DeliverAsync(SettleOnboardingTaskCalculationIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.ReferenceId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.onboarding.v1.OnboardingTaskCalculationService/ExplainOnboardingTaskCalculation and hands what it keeps to the sink of its kind.</summary>
internal static class SettleOnboardingTaskCalculationId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient(channel);
        var reply = await source.ExplainOnboardingTaskCalculationAsync(new ApiOnboarding.ExplainOnboardingTaskCalculationRequest() { OnboardingTaskId = "default-settle-onboarding-task-calculation-id" }, cancellationToken: token);
        var record = new SettleOnboardingTaskCalculationIdRecord
        {
            ReferenceId = reply.Outcome.ReferenceId,
            CorrelationId = reply.Result.CorrelationId,
            OnboardingTaskId = Text.Normalize(reply.OnboardingTask.OnboardingTaskId),
            Status = reply.Outcome.Status,
        };
        await RouteSettleOnboardingTaskCalculationIdAsync("recommendation-set-query", record, channel, token);
    }

    private static async Task RouteSettleOnboardingTaskCalculationIdAsync(string kind, SettleOnboardingTaskCalculationIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        ISettleOnboardingTaskCalculationIdWriter sink = kind switch
        {
            "customer" => new SettleOnboardingTaskCalculationIdCustomerWriter(new ApiCustomers.CustomerService.CustomerServiceClient(channel)),
            "recommendation-set-query" => new SettleOnboardingTaskCalculationIdRecommendationSetQuerySink(new ApiRecommendations.RecommendationSetQueryService.RecommendationSetQueryServiceClient(channel)),
            _ => new SettleOnboardingTaskCalculationIdLogOnly(),
        };
        await sink.DeliverAsync(record, token);
    }
}
