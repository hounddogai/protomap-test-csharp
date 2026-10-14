namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleSegment;

using Acme.Workforce.Compensation.MeritCycle.CycleSegment;

/// <summary>Checks of cycle segment code, run by the project's runner.</summary>
public static class AdaptiveMeritCycleThresholdChecks
{
    public static void AdaptiveMeritCycleThresholdStartsInRequested()
    {
        var item = AdaptiveMeritCycleThreshold.NewAdaptiveMeritCycleThreshold("code-972", "name-435");
        Check.Equal(item.Status, AdaptiveMeritCycleThresholdStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveMeritCycleThresholdMovesToRevoked()
    {
        var item = AdaptiveMeritCycleThreshold.NewAdaptiveMeritCycleThreshold("code-972", "name-435");
        try
        {
            item.MoveTo(AdaptiveMeritCycleThresholdStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveMeritCycleThresholdStatus.Revoked, "status");
    }

    public static void AdaptiveMeritCycleThresholdRejectsMovesFromExpired()
    {
        var item = AdaptiveMeritCycleThreshold.NewAdaptiveMeritCycleThreshold("code-972", "name-435");
        item.Status = AdaptiveMeritCycleThresholdStatus.Expired;
        Check.Fails(() => item.MoveTo(AdaptiveMeritCycleThresholdStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AdaptiveMeritCycleThresholdStartsInRequested();
        AdaptiveMeritCycleThresholdMovesToRevoked();
        AdaptiveMeritCycleThresholdRejectsMovesFromExpired();
    }
}
