using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiBenefits = Acme.Benefits.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Ops.PayrollopsSweeper;

/// <summary>Where ForwardBenefitPlanId hands each record.</summary>
internal interface IForwardBenefitPlanIdTarget
{
    Task WriteAsync(ForwardBenefitPlanIdRecord record, CancellationToken token);
}

/// <summary>What ForwardBenefitPlanId keeps of each GetBenefitPlan response.</summary>
internal sealed class ForwardBenefitPlanIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
    public string BenefitPlanId { get; init; } = "";
}

/// <summary>Sends records to acme.documents.v1.DocumentValidationService.</summary>
internal sealed class ForwardBenefitPlanIdDocumentValidationTarget(ApiDocuments.DocumentValidationService.DocumentValidationServiceClient client) : IForwardBenefitPlanIdTarget
{
    public async Task WriteAsync(ForwardBenefitPlanIdRecord record, CancellationToken token)
    {
        await client.ValidateDocumentAsync(new ApiDocuments.ValidateDocumentRequest { Document = new ApiDocuments.Document { Notes = record.CorrelationId }, Reason = record.Message }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.recruiting.v1.OfferCalculationService.</summary>
internal sealed class ForwardBenefitPlanIdOfferCalculationWriter(ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient client) : IForwardBenefitPlanIdTarget
{
    public async Task WriteAsync(ForwardBenefitPlanIdRecord record, CancellationToken token)
    {
        await client.ExplainOfferCalculationAsync(new ApiRecruiting.ExplainOfferCalculationRequest { Offer = new ApiRecruiting.Offer { Number = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.onboarding.v1.OnboardingTaskApprovalService.</summary>
internal sealed class ForwardBenefitPlanIdOnboardingTaskApprovalRelay : IForwardBenefitPlanIdTarget
{
    private readonly ApiOnboarding.OnboardingTaskApprovalService.OnboardingTaskApprovalServiceClient _client;

    public ForwardBenefitPlanIdOnboardingTaskApprovalRelay(ApiOnboarding.OnboardingTaskApprovalService.OnboardingTaskApprovalServiceClient client) => _client = client;

    public async Task WriteAsync(ForwardBenefitPlanIdRecord record, CancellationToken token)
    {
        await _client.ApproveOnboardingTaskAsync(new ApiOnboarding.ApproveOnboardingTaskRequest { Reason = record.BenefitPlanId, Details = new ApiOnboarding.ApproveOnboardingTaskRequest.Types.Details { ApproverEmail = record.Message }, OnboardingTask = new ApiOnboarding.OnboardingTask { OnboardingTaskId = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.benefits.v1.BenefitPlanService/GetBenefitPlan and hands what it keeps to the sink of its kind.</summary>
internal static class ForwardBenefitPlanId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiBenefits.BenefitPlanService.BenefitPlanServiceClient(channel);
        var found = await source.GetBenefitPlanAsync(new ApiBenefits.GetBenefitPlanRequest() { BenefitPlanId = "main-forward-benefit-plan-id" }, cancellationToken: token);
        var record = new ForwardBenefitPlanIdRecord
        {
            CorrelationId = Text.Normalize(found.Result.CorrelationId),
            Message = found.Result.Message,
            BenefitPlanId = Text.Normalize(found.BenefitPlan.BenefitPlanId),
        };
        var sinks = new Dictionary<string, IForwardBenefitPlanIdTarget>
        {
            ["document-validation"] = new ForwardBenefitPlanIdDocumentValidationTarget(new ApiDocuments.DocumentValidationService.DocumentValidationServiceClient(channel)),
            ["offer-calculation"] = new ForwardBenefitPlanIdOfferCalculationWriter(new ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient(channel)),
            ["onboarding-task-approval"] = new ForwardBenefitPlanIdOnboardingTaskApprovalRelay(new ApiOnboarding.OnboardingTaskApprovalService.OnboardingTaskApprovalServiceClient(channel)),
        };
        await RouteForwardBenefitPlanIdAsync("document-validation", record, sinks, token);
    }

    private static async Task RouteForwardBenefitPlanIdAsync(string kind, ForwardBenefitPlanIdRecord record, IReadOnlyDictionary<string, IForwardBenefitPlanIdTarget> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.WriteAsync(record, token);
    }
}
