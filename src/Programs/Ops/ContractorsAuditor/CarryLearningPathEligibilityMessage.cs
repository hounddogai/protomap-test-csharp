using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiLearning = Acme.Learning.V1;

namespace Acme.Workforce.Ops.ContractorsAuditor;

/// <summary>What CarryLearningPathEligibilityMessage keeps of each CheckLearningPathEligibility response.</summary>
internal sealed class CarryLearningPathEligibilityMessageRecord
{
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
    public string LearningPathId { get; init; } = "";
    public string OutcomeStatus { get; init; } = "";
}

/// <summary>Passes records to acme.learning.v1.LearningPathCalculationService.</summary>
internal sealed class CarryLearningPathEligibilityMessageLearningPathCalculationWriter(ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient client)
{
    public async Task DeliverAsync(CarryLearningPathEligibilityMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiLearning.ExplainLearningPathCalculationRequest { Details = new ApiLearning.ExplainLearningPathCalculationRequest.Types.Details { Locale = record.LearningPathId }, Reason = record.Status, LearningPath = new ApiLearning.LearningPath { Name = record.Message } }, token, client.ExplainLearningPathCalculationAsync);
    }
}

/// <summary>Reads acme.learning.v1.LearningPathValidationService/CheckLearningPathEligibility and hands what it keeps to acme.learning.v1.LearningPathCalculationService.</summary>
internal static class CarryLearningPathEligibilityMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiLearning.LearningPathValidationService.LearningPathValidationServiceClient(channel);
        var got = await source.CheckLearningPathEligibilityAsync(new ApiLearning.CheckLearningPathEligibilityRequest() { Reason = "main-carry-learning-path-eligibility-message" }, cancellationToken: token);
        var record = new CarryLearningPathEligibilityMessageRecord
        {
            Message = Text.Normalize(got.Result.Message),
            Status = got.Result.Status,
            LearningPathId = got.LearningPath.LearningPathId,
            OutcomeStatus = got.Outcome.Status,
        };
        await new CarryLearningPathEligibilityMessageLearningPathCalculationWriter(new ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient(channel)).DeliverAsync(record, token);
    }
}
