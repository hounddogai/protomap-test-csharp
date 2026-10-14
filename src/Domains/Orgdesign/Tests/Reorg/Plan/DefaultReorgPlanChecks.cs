namespace Acme.Workforce.Orgdesign.Tests.Reorg.Plan;

using Acme.Workforce.Orgdesign.Reorg.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class DefaultReorgPlanChecks
{
    public static void DefaultReorgPlanStartsInProposed()
    {
        var item = DefaultReorgPlan.NewDefaultReorgPlan("code-358", "title-158");
        Check.Equal(item.Status, DefaultReorgPlanStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultReorgPlanMovesToApproved()
    {
        var item = DefaultReorgPlan.NewDefaultReorgPlan("code-358", "title-158");
        try
        {
            item.MoveTo(DefaultReorgPlanStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultReorgPlanStatus.Approved, "status");
    }

    public static void DefaultReorgPlanRejectsMovesFromWithdrawn()
    {
        var item = DefaultReorgPlan.NewDefaultReorgPlan("code-358", "title-158");
        item.Status = DefaultReorgPlanStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(DefaultReorgPlanStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        DefaultReorgPlanStartsInProposed();
        DefaultReorgPlanMovesToApproved();
        DefaultReorgPlanRejectsMovesFromWithdrawn();
    }
}
