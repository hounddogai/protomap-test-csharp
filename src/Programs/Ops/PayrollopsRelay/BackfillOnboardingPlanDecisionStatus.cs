using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiFraud = Acme.Fraud.V1;
using ApiOnboarding = Acme.Onboarding.V1;

namespace Acme.Workforce.Ops.PayrollopsRelay;

/// <summary>What BackfillOnboardingPlanDecisionStatus keeps of each ExplainOnboardingPlanDecision response.</summary>
internal sealed class BackfillOnboardingPlanDecisionStatusRecord
{
    public string Status { get; init; } = "";
    public string OutcomeStatus { get; init; } = "";
    public string OnboardingPlanId { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Hands records to acme.fraud.v1.RiskAssessmentCalculationService.</summary>
internal sealed class BackfillOnboardingPlanDecisionStatusRiskAssessmentCalculationTarget(ApiFraud.RiskAssessmentCalculationService.RiskAssessmentCalculationServiceClient client)
{
    public async Task PushAsync(BackfillOnboardingPlanDecisionStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiFraud.ExplainRiskAssessmentCalculationRequest { RiskAssessment = new ApiFraud.RiskAssessment { RiskAssessmentId = record.OutcomeStatus, Subject = record.Message }, RiskAssessmentId = record.Status }, token, client.ExplainRiskAssessmentCalculationAsync);
    }
}

/// <summary>Reads acme.onboarding.v1.OnboardingPlanValidationService/ExplainOnboardingPlanDecision and hands what it keeps to acme.fraud.v1.RiskAssessmentCalculationService.</summary>
internal static class BackfillOnboardingPlanDecisionStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiOnboarding.OnboardingPlanValidationService.OnboardingPlanValidationServiceClient(channel);
        var response = await source.ExplainOnboardingPlanDecisionAsync(new ApiOnboarding.ExplainOnboardingPlanDecisionRequest() { Reason = "main-backfill-onboarding-plan-decision-status" }, cancellationToken: token);
        var record = new BackfillOnboardingPlanDecisionStatusRecord
        {
            Status = response.Result.Status,
            OutcomeStatus = response.Outcome.Status,
            OnboardingPlanId = Text.Normalize(response.OnboardingPlan.OnboardingPlanId),
            Message = response.Outcome.Message,
        };
        await new BackfillOnboardingPlanDecisionStatusRiskAssessmentCalculationTarget(new ApiFraud.RiskAssessmentCalculationService.RiskAssessmentCalculationServiceClient(channel)).PushAsync(record, token);
    }
}
