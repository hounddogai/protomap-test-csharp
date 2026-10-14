using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCustomers = Acme.Customers.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Ops.CompensationSync;

/// <summary>Where ReplayEntitlementMessage hands each record.</summary>
internal interface IReplayEntitlementMessageDestination
{
    Task DeliverAsync(ReplayEntitlementMessageRecord record, CancellationToken token);
}

/// <summary>What ReplayEntitlementMessage keeps of each ValidateEntitlement response.</summary>
internal sealed class ReplayEntitlementMessageRecord
{
    public string Message { get; init; } = "";
    public string ReferenceId { get; init; } = "";
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Hands records to acme.learning.v1.LearningPathApprovalService.</summary>
internal sealed class ReplayEntitlementMessageLearningPathApprovalTarget : IReplayEntitlementMessageDestination
{
    private readonly ApiLearning.LearningPathApprovalService.LearningPathApprovalServiceClient _client;

    public ReplayEntitlementMessageLearningPathApprovalTarget(ApiLearning.LearningPathApprovalService.LearningPathApprovalServiceClient client) => _client = client;

    public async Task DeliverAsync(ReplayEntitlementMessageRecord record, CancellationToken token)
    {
        await _client.EscalateLearningPathAsync(new ApiLearning.EscalateLearningPathRequest { Details = new ApiLearning.EscalateLearningPathRequest.Types.Details { Comment = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.customers.v1.CustomerCalculationService.</summary>
internal sealed class ReplayEntitlementMessageCustomerCalculationTarget(ApiCustomers.CustomerCalculationService.CustomerCalculationServiceClient client) : IReplayEntitlementMessageDestination
{
    public async Task DeliverAsync(ReplayEntitlementMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiCustomers.RecalculateCustomersRequest { Details = new ApiCustomers.RecalculateCustomersRequest.Types.Details { CurrencyCode = record.ReferenceId }, Customer = new ApiCustomers.Customer { CustomerId = record.CorrelationId } }, token, client.RecalculateCustomersAsync);
    }
}

/// <summary>Reads acme.licensing.v1.EntitlementValidationService/ValidateEntitlement and hands what it keeps to the sink of its kind.</summary>
internal static class ReplayEntitlementMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient(channel);
        var reply = await source.ValidateEntitlementAsync(new ApiLicensing.ValidateEntitlementRequest() { EntitlementId = "live-replay-entitlement-message" }, cancellationToken: token);
        var record = new ReplayEntitlementMessageRecord
        {
            Message = reply.Result.Message,
            ReferenceId = reply.Outcome.ReferenceId,
            Status = reply.Outcome.Status,
            CorrelationId = reply.Result.CorrelationId,
        };
        await ActOnReplayEntitlementMessageAsync("customer-calculation", record, channel, token);
    }

    private static async Task ActOnReplayEntitlementMessageAsync(string kind, ReplayEntitlementMessageRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["learning-path-approval"] = () => new ReplayEntitlementMessageLearningPathApprovalTarget(new ApiLearning.LearningPathApprovalService.LearningPathApprovalServiceClient(channel)).DeliverAsync(record, token),
            ["customer-calculation"] = () => new ReplayEntitlementMessageCustomerCalculationTarget(new ApiCustomers.CustomerCalculationService.CustomerCalculationServiceClient(channel)).DeliverAsync(record, token),
        };
        await actions[kind]();
    }
}
