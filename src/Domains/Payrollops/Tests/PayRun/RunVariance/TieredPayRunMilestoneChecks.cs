namespace Acme.Workforce.Payrollops.Tests.PayRun.RunVariance;

using Acme.Workforce.Payrollops.PayRun.RunVariance;

/// <summary>Checks of run variance code, run by the project's runner.</summary>
public static class TieredPayRunMilestoneChecks
{
    public static void TieredPayRunMilestoneStartsInIdle()
    {
        var item = TieredPayRunMilestone.NewTieredPayRunMilestone("code-648", "label-331");
        Check.Equal(item.Status, TieredPayRunMilestoneStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredPayRunMilestoneMovesToWarming()
    {
        var item = TieredPayRunMilestone.NewTieredPayRunMilestone("code-648", "label-331");
        try
        {
            item.MoveTo(TieredPayRunMilestoneStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredPayRunMilestoneStatus.Warming, "status");
    }

    public static void TieredPayRunMilestoneRejectsMovesFromStopped()
    {
        var item = TieredPayRunMilestone.NewTieredPayRunMilestone("code-648", "label-331");
        item.Status = TieredPayRunMilestoneStatus.Stopped;
        Check.Fails(() => item.MoveTo(TieredPayRunMilestoneStatus.Idle), "move");
    }

    public static void RunAll()
    {
        TieredPayRunMilestoneStartsInIdle();
        TieredPayRunMilestoneMovesToWarming();
        TieredPayRunMilestoneRejectsMovesFromStopped();
    }
}
