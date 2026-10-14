namespace Acme.Workforce.Engagement.Tests.ActionPlan.Metric;

using Acme.Workforce.Engagement.ActionPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class AdaptiveActionPlanReviewChecks
{
    public static void AdaptiveActionPlanReviewStartsInCreated()
    {
        var item = AdaptiveActionPlanReview.NewAdaptiveActionPlanReview("code-606", "title-540");
        Check.Equal(item.Status, AdaptiveActionPlanReviewStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveActionPlanReviewMovesToValidated()
    {
        var item = AdaptiveActionPlanReview.NewAdaptiveActionPlanReview("code-606", "title-540");
        try
        {
            item.MoveTo(AdaptiveActionPlanReviewStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveActionPlanReviewStatus.Validated, "status");
    }

    public static void AdaptiveActionPlanReviewRejectsMovesFromRetired()
    {
        var item = AdaptiveActionPlanReview.NewAdaptiveActionPlanReview("code-606", "title-540");
        item.Status = AdaptiveActionPlanReviewStatus.Retired;
        Check.Fails(() => item.MoveTo(AdaptiveActionPlanReviewStatus.Created), "move");
    }

    public static void RunAll()
    {
        AdaptiveActionPlanReviewStartsInCreated();
        AdaptiveActionPlanReviewMovesToValidated();
        AdaptiveActionPlanReviewRejectsMovesFromRetired();
    }
}
