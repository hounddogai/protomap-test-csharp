namespace Acme.Workforce.Compensation.Tests.MeritCycle.OverrideItem;

using Acme.Workforce.Compensation.MeritCycle.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class MeritCycleSnapshotChecks
{
    public static void MeritCycleSnapshotStartsInIdle()
    {
        var item = MeritCycleSnapshot.NewMeritCycleSnapshot("code-799", "label-457");
        Check.Equal(item.Status, MeritCycleSnapshotStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleSnapshotMovesToReady()
    {
        var item = MeritCycleSnapshot.NewMeritCycleSnapshot("code-799", "label-457");
        try
        {
            item.MoveTo(MeritCycleSnapshotStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleSnapshotStatus.Ready, "status");
    }

    public static void MeritCycleSnapshotRejectsMovesFromStopped()
    {
        var item = MeritCycleSnapshot.NewMeritCycleSnapshot("code-799", "label-457");
        item.Status = MeritCycleSnapshotStatus.Stopped;
        Check.Fails(() => item.MoveTo(MeritCycleSnapshotStatus.Idle), "move");
    }

    public static void RunAll()
    {
        MeritCycleSnapshotStartsInIdle();
        MeritCycleSnapshotMovesToReady();
        MeritCycleSnapshotRejectsMovesFromStopped();
    }
}
