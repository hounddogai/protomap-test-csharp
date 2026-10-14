namespace Acme.Workforce.Orgdesign.Tests.Reorg.Profile;

using Acme.Workforce.Orgdesign.Reorg.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class ReorgPlanChecks
{
    public static void ReorgPlanStartsInIdle()
    {
        var item = ReorgPlan.NewReorgPlan("code-321", "label-454");
        Check.Equal(item.Status, ReorgPlanStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReorgPlanMovesToReady()
    {
        var item = ReorgPlan.NewReorgPlan("code-321", "label-454");
        try
        {
            item.MoveTo(ReorgPlanStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReorgPlanStatus.Ready, "status");
    }

    public static void ReorgPlanRejectsMovesFromStopped()
    {
        var item = ReorgPlan.NewReorgPlan("code-321", "label-454");
        item.Status = ReorgPlanStatus.Stopped;
        Check.Fails(() => item.MoveTo(ReorgPlanStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ReorgPlanStartsInIdle();
        ReorgPlanMovesToReady();
        ReorgPlanRejectsMovesFromStopped();
    }
}
