namespace Acme.Workforce.Compensation.Tests.MeritCycle.Metric;

using Acme.Workforce.Compensation.MeritCycle.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class TieredMeritCycleCheckpointChecks
{
    public static void TieredMeritCycleCheckpointStartsInRequested()
    {
        var item = TieredMeritCycleCheckpoint.NewTieredMeritCycleCheckpoint("code-661", "title-457");
        Check.Equal(item.Status, TieredMeritCycleCheckpointStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredMeritCycleCheckpointMovesToGranted()
    {
        var item = TieredMeritCycleCheckpoint.NewTieredMeritCycleCheckpoint("code-661", "title-457");
        try
        {
            item.MoveTo(TieredMeritCycleCheckpointStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredMeritCycleCheckpointStatus.Granted, "status");
    }

    public static void TieredMeritCycleCheckpointRejectsMovesFromExpired()
    {
        var item = TieredMeritCycleCheckpoint.NewTieredMeritCycleCheckpoint("code-661", "title-457");
        item.Status = TieredMeritCycleCheckpointStatus.Expired;
        Check.Fails(() => item.MoveTo(TieredMeritCycleCheckpointStatus.Requested), "move");
    }

    public static void RunAll()
    {
        TieredMeritCycleCheckpointStartsInRequested();
        TieredMeritCycleCheckpointMovesToGranted();
        TieredMeritCycleCheckpointRejectsMovesFromExpired();
    }
}
