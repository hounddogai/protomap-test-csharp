namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Target;

using Acme.Workforce.Talent.SuccessionPlan.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class TieredSuccessionPlanMilestoneChecks
{
    public static void TieredSuccessionPlanMilestoneStartsInOpen()
    {
        var item = TieredSuccessionPlanMilestone.NewTieredSuccessionPlanMilestone("code-896", "name-883");
        Check.Equal(item.Status, TieredSuccessionPlanMilestoneStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredSuccessionPlanMilestoneMovesToInReview()
    {
        var item = TieredSuccessionPlanMilestone.NewTieredSuccessionPlanMilestone("code-896", "name-883");
        try
        {
            item.MoveTo(TieredSuccessionPlanMilestoneStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredSuccessionPlanMilestoneStatus.InReview, "status");
    }

    public static void TieredSuccessionPlanMilestoneRejectsMovesFromArchived()
    {
        var item = TieredSuccessionPlanMilestone.NewTieredSuccessionPlanMilestone("code-896", "name-883");
        item.Status = TieredSuccessionPlanMilestoneStatus.Archived;
        Check.Fails(() => item.MoveTo(TieredSuccessionPlanMilestoneStatus.Open), "move");
    }

    public static void RunAll()
    {
        TieredSuccessionPlanMilestoneStartsInOpen();
        TieredSuccessionPlanMilestoneMovesToInReview();
        TieredSuccessionPlanMilestoneRejectsMovesFromArchived();
    }
}
