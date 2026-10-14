namespace Acme.Workforce.Engagement.Tests.ActionPlan.Mapping;

using Acme.Workforce.Engagement.ActionPlan.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class ActionPlanSummaryChecks
{
    public static void ActionPlanSummaryStartsInOpen()
    {
        var item = ActionPlanSummary.NewActionPlanSummary("code-506", "name-294");
        Check.Equal(item.Status, ActionPlanSummaryStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanSummaryMovesToInReview()
    {
        var item = ActionPlanSummary.NewActionPlanSummary("code-506", "name-294");
        try
        {
            item.MoveTo(ActionPlanSummaryStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanSummaryStatus.InReview, "status");
    }

    public static void ActionPlanSummaryRejectsMovesFromArchived()
    {
        var item = ActionPlanSummary.NewActionPlanSummary("code-506", "name-294");
        item.Status = ActionPlanSummaryStatus.Archived;
        Check.Fails(() => item.MoveTo(ActionPlanSummaryStatus.Open), "move");
    }

    public static void RunAll()
    {
        ActionPlanSummaryStartsInOpen();
        ActionPlanSummaryMovesToInReview();
        ActionPlanSummaryRejectsMovesFromArchived();
    }
}
