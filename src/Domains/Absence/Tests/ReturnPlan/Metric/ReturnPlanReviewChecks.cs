namespace Acme.Workforce.Absence.Tests.ReturnPlan.Metric;

using Acme.Workforce.Absence.ReturnPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ReturnPlanReviewChecks
{
    public static void ReturnPlanReviewStartsInProposed()
    {
        var item = ReturnPlanReview.NewReturnPlanReview("code-929", "name-130");
        Check.Equal(item.Status, ReturnPlanReviewStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReturnPlanReviewMovesToApproved()
    {
        var item = ReturnPlanReview.NewReturnPlanReview("code-929", "name-130");
        try
        {
            item.MoveTo(ReturnPlanReviewStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReturnPlanReviewStatus.Approved, "status");
    }

    public static void ReturnPlanReviewRejectsMovesFromWithdrawn()
    {
        var item = ReturnPlanReview.NewReturnPlanReview("code-929", "name-130");
        item.Status = ReturnPlanReviewStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ReturnPlanReviewStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ReturnPlanReviewStartsInProposed();
        ReturnPlanReviewMovesToApproved();
        ReturnPlanReviewRejectsMovesFromWithdrawn();
    }
}
