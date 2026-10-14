namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleSegment;

using Acme.Workforce.Compensation.MeritCycle.CycleSegment;

/// <summary>Checks of cycle segment code, run by the project's runner.</summary>
public static class AdaptiveMeritCycleMappingChecks
{
    public static void AdaptiveMeritCycleMappingStartsInIdle()
    {
        var item = AdaptiveMeritCycleMapping.NewAdaptiveMeritCycleMapping("code-243", "name-690");
        Check.Equal(item.Status, AdaptiveMeritCycleMappingStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveMeritCycleMappingMovesToWarming()
    {
        var item = AdaptiveMeritCycleMapping.NewAdaptiveMeritCycleMapping("code-243", "name-690");
        try
        {
            item.MoveTo(AdaptiveMeritCycleMappingStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveMeritCycleMappingStatus.Warming, "status");
    }

    public static void AdaptiveMeritCycleMappingRejectsMovesFromStopped()
    {
        var item = AdaptiveMeritCycleMapping.NewAdaptiveMeritCycleMapping("code-243", "name-690");
        item.Status = AdaptiveMeritCycleMappingStatus.Stopped;
        Check.Fails(() => item.MoveTo(AdaptiveMeritCycleMappingStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AdaptiveMeritCycleMappingStartsInIdle();
        AdaptiveMeritCycleMappingMovesToWarming();
        AdaptiveMeritCycleMappingRejectsMovesFromStopped();
    }
}
