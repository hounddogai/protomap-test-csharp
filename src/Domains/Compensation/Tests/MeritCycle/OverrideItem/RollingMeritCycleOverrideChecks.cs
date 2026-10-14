namespace Acme.Workforce.Compensation.Tests.MeritCycle.OverrideItem;

using Acme.Workforce.Compensation.MeritCycle.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RollingMeritCycleOverrideChecks
{
    public static void RollingMeritCycleOverrideStartsInIdle()
    {
        var item = RollingMeritCycleOverride.NewRollingMeritCycleOverride("code-282", "name-900");
        Check.Equal(item.Status, RollingMeritCycleOverrideStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingMeritCycleOverrideMovesToDraining()
    {
        var item = RollingMeritCycleOverride.NewRollingMeritCycleOverride("code-282", "name-900");
        try
        {
            item.MoveTo(RollingMeritCycleOverrideStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingMeritCycleOverrideStatus.Draining, "status");
    }

    public static void RollingMeritCycleOverrideRejectsMovesFromStopped()
    {
        var item = RollingMeritCycleOverride.NewRollingMeritCycleOverride("code-282", "name-900");
        item.Status = RollingMeritCycleOverrideStatus.Stopped;
        Check.Fails(() => item.MoveTo(RollingMeritCycleOverrideStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RollingMeritCycleOverrideStartsInIdle();
        RollingMeritCycleOverrideMovesToDraining();
        RollingMeritCycleOverrideRejectsMovesFromStopped();
    }
}
