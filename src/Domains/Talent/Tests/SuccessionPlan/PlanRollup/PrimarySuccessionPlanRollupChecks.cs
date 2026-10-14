namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanRollup;

using Acme.Workforce.Talent.SuccessionPlan.PlanRollup;

/// <summary>Checks of plan rollup code, run by the project's runner.</summary>
public static class PrimarySuccessionPlanRollupChecks
{
    public static void PrimarySuccessionPlanRollupStartsInOpen()
    {
        var item = PrimarySuccessionPlanRollup.NewPrimarySuccessionPlanRollup("code-697", "label-208");
        Check.Equal(item.Status, PrimarySuccessionPlanRollupStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimarySuccessionPlanRollupMovesToArchived()
    {
        var item = PrimarySuccessionPlanRollup.NewPrimarySuccessionPlanRollup("code-697", "label-208");
        try
        {
            item.MoveTo(PrimarySuccessionPlanRollupStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimarySuccessionPlanRollupStatus.Archived, "status");
    }

    public static void PrimarySuccessionPlanRollupRejectsMovesFromArchived()
    {
        var item = PrimarySuccessionPlanRollup.NewPrimarySuccessionPlanRollup("code-697", "label-208");
        item.Status = PrimarySuccessionPlanRollupStatus.Archived;
        Check.Fails(() => item.MoveTo(PrimarySuccessionPlanRollupStatus.Open), "move");
    }

    public static void RunAll()
    {
        PrimarySuccessionPlanRollupStartsInOpen();
        PrimarySuccessionPlanRollupMovesToArchived();
        PrimarySuccessionPlanRollupRejectsMovesFromArchived();
    }
}
