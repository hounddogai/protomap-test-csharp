namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleReview;

using Acme.Workforce.Compensation.MeritCycle.CycleReview;

/// <summary>Checks of cycle review code, run by the project's runner.</summary>
public static class WeightedMeritCycleTierChecks
{
    public static void WeightedMeritCycleTierStartsInIdle()
    {
        var item = WeightedMeritCycleTier.NewWeightedMeritCycleTier("code-176", "label-675");
        Check.Equal(item.Status, WeightedMeritCycleTierStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedMeritCycleTierMovesToReady()
    {
        var item = WeightedMeritCycleTier.NewWeightedMeritCycleTier("code-176", "label-675");
        try
        {
            item.MoveTo(WeightedMeritCycleTierStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedMeritCycleTierStatus.Ready, "status");
    }

    public static void WeightedMeritCycleTierRejectsMovesFromStopped()
    {
        var item = WeightedMeritCycleTier.NewWeightedMeritCycleTier("code-176", "label-675");
        item.Status = WeightedMeritCycleTierStatus.Stopped;
        Check.Fails(() => item.MoveTo(WeightedMeritCycleTierStatus.Idle), "move");
    }

    public static void RunAll()
    {
        WeightedMeritCycleTierStartsInIdle();
        WeightedMeritCycleTierMovesToReady();
        WeightedMeritCycleTierRejectsMovesFromStopped();
    }
}
