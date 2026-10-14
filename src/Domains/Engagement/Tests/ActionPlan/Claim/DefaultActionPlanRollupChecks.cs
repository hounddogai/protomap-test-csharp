namespace Acme.Workforce.Engagement.Tests.ActionPlan.Claim;

using Acme.Workforce.Engagement.ActionPlan.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class DefaultActionPlanRollupChecks
{
    public static void DefaultActionPlanRollupStartsInOpen()
    {
        var item = DefaultActionPlanRollup.NewDefaultActionPlanRollup("code-883", "label-183");
        Check.Equal(item.Status, DefaultActionPlanRollupStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultActionPlanRollupMovesToInReview()
    {
        var item = DefaultActionPlanRollup.NewDefaultActionPlanRollup("code-883", "label-183");
        try
        {
            item.MoveTo(DefaultActionPlanRollupStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultActionPlanRollupStatus.InReview, "status");
    }

    public static void DefaultActionPlanRollupRejectsMovesFromArchived()
    {
        var item = DefaultActionPlanRollup.NewDefaultActionPlanRollup("code-883", "label-183");
        item.Status = DefaultActionPlanRollupStatus.Archived;
        Check.Fails(() => item.MoveTo(DefaultActionPlanRollupStatus.Open), "move");
    }

    public static void RunAll()
    {
        DefaultActionPlanRollupStartsInOpen();
        DefaultActionPlanRollupMovesToInReview();
        DefaultActionPlanRollupRejectsMovesFromArchived();
    }
}
