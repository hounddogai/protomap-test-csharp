namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Batch;

using Acme.Workforce.Talent.SuccessionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class SuccessionPlanReviewChecks
{
    public static void SuccessionPlanReviewStartsInRequested()
    {
        var item = SuccessionPlanReview.NewSuccessionPlanReview("code-455", "title-185");
        Check.Equal(item.Status, SuccessionPlanReviewStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SuccessionPlanReviewMovesToGranted()
    {
        var item = SuccessionPlanReview.NewSuccessionPlanReview("code-455", "title-185");
        try
        {
            item.MoveTo(SuccessionPlanReviewStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SuccessionPlanReviewStatus.Granted, "status");
    }

    public static void SuccessionPlanReviewRejectsMovesFromExpired()
    {
        var item = SuccessionPlanReview.NewSuccessionPlanReview("code-455", "title-185");
        item.Status = SuccessionPlanReviewStatus.Expired;
        Check.Fails(() => item.MoveTo(SuccessionPlanReviewStatus.Requested), "move");
    }

    public static void RunAll()
    {
        SuccessionPlanReviewStartsInRequested();
        SuccessionPlanReviewMovesToGranted();
        SuccessionPlanReviewRejectsMovesFromExpired();
    }
}
