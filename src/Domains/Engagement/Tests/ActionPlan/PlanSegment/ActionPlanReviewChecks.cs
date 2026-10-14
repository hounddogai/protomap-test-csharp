namespace Acme.Workforce.Engagement.Tests.ActionPlan.PlanSegment;

using Acme.Workforce.Engagement.ActionPlan.PlanSegment;

/// <summary>Checks of plan segment code, run by the project's runner.</summary>
public static class ActionPlanReviewChecks
{
    public static void ActionPlanReviewStartsInPlanned()
    {
        var item = ActionPlanReview.NewActionPlanReview("code-481", "label-18");
        Check.Equal(item.Status, ActionPlanReviewStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanReviewMovesToInProgress()
    {
        var item = ActionPlanReview.NewActionPlanReview("code-481", "label-18");
        try
        {
            item.MoveTo(ActionPlanReviewStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanReviewStatus.InProgress, "status");
    }

    public static void ActionPlanReviewRejectsMovesFromCancelled()
    {
        var item = ActionPlanReview.NewActionPlanReview("code-481", "label-18");
        item.Status = ActionPlanReviewStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ActionPlanReviewStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ActionPlanReviewStartsInPlanned();
        ActionPlanReviewMovesToInProgress();
        ActionPlanReviewRejectsMovesFromCancelled();
    }
}
