namespace Acme.Workforce.Compensation.Tests.MeritCycle.Mapping;

using Acme.Workforce.Compensation.MeritCycle.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class LegacyMeritCycleCheckpointChecks
{
    public static void LegacyMeritCycleCheckpointStartsInIdle()
    {
        var item = LegacyMeritCycleCheckpoint.NewLegacyMeritCycleCheckpoint("code-105", "name-501");
        Check.Equal(item.Status, LegacyMeritCycleCheckpointStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyMeritCycleCheckpointMovesToReady()
    {
        var item = LegacyMeritCycleCheckpoint.NewLegacyMeritCycleCheckpoint("code-105", "name-501");
        try
        {
            item.MoveTo(LegacyMeritCycleCheckpointStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyMeritCycleCheckpointStatus.Ready, "status");
    }

    public static void LegacyMeritCycleCheckpointRejectsMovesFromStopped()
    {
        var item = LegacyMeritCycleCheckpoint.NewLegacyMeritCycleCheckpoint("code-105", "name-501");
        item.Status = LegacyMeritCycleCheckpointStatus.Stopped;
        Check.Fails(() => item.MoveTo(LegacyMeritCycleCheckpointStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LegacyMeritCycleCheckpointStartsInIdle();
        LegacyMeritCycleCheckpointMovesToReady();
        LegacyMeritCycleCheckpointRejectsMovesFromStopped();
    }
}
