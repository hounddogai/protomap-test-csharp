namespace Acme.Workforce.Assets.Tests.Refresh.Plan;

using Acme.Workforce.Assets.Refresh.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RefreshPlanChecks
{
    public static void RefreshPlanStartsInPlanned()
    {
        var item = RefreshPlan.NewRefreshPlan("code-819", "label-246");
        Check.Equal(item.Status, RefreshPlanStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RefreshPlanMovesToScheduled()
    {
        var item = RefreshPlan.NewRefreshPlan("code-819", "label-246");
        try
        {
            item.MoveTo(RefreshPlanStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RefreshPlanStatus.Scheduled, "status");
    }

    public static void RefreshPlanRejectsMovesFromCancelled()
    {
        var item = RefreshPlan.NewRefreshPlan("code-819", "label-246");
        item.Status = RefreshPlanStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RefreshPlanStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RefreshPlanStartsInPlanned();
        RefreshPlanMovesToScheduled();
        RefreshPlanRejectsMovesFromCancelled();
    }
}
