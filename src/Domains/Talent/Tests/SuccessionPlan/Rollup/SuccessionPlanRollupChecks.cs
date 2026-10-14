namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Rollup;

using Acme.Workforce.Talent.SuccessionPlan.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SuccessionPlanRollupChecks
{
    public static void SuccessionPlanRollupStartsInOpen()
    {
        var item = SuccessionPlanRollup.NewSuccessionPlanRollup("code-961", "label-167");
        Check.Equal(item.Status, SuccessionPlanRollupStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SuccessionPlanRollupMovesToInReview()
    {
        var item = SuccessionPlanRollup.NewSuccessionPlanRollup("code-961", "label-167");
        try
        {
            item.MoveTo(SuccessionPlanRollupStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SuccessionPlanRollupStatus.InReview, "status");
    }

    public static void SuccessionPlanRollupRejectsMovesFromArchived()
    {
        var item = SuccessionPlanRollup.NewSuccessionPlanRollup("code-961", "label-167");
        item.Status = SuccessionPlanRollupStatus.Archived;
        Check.Fails(() => item.MoveTo(SuccessionPlanRollupStatus.Open), "move");
    }

    public static void RunAll()
    {
        SuccessionPlanRollupStartsInOpen();
        SuccessionPlanRollupMovesToInReview();
        SuccessionPlanRollupRejectsMovesFromArchived();
    }
}
