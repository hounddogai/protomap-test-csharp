namespace Acme.Workforce.Compensation.Tests.MeritCycle.Mapping;

using Acme.Workforce.Compensation.MeritCycle.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class ScheduledMeritCycleSnapshotChecks
{
    public static void ScheduledMeritCycleSnapshotStartsInOpen()
    {
        var item = ScheduledMeritCycleSnapshot.NewScheduledMeritCycleSnapshot("code-872", "name-822");
        Check.Equal(item.Status, ScheduledMeritCycleSnapshotStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledMeritCycleSnapshotMovesToReopened()
    {
        var item = ScheduledMeritCycleSnapshot.NewScheduledMeritCycleSnapshot("code-872", "name-822");
        try
        {
            item.MoveTo(ScheduledMeritCycleSnapshotStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledMeritCycleSnapshotStatus.Reopened, "status");
    }

    public static void ScheduledMeritCycleSnapshotRejectsMovesFromArchived()
    {
        var item = ScheduledMeritCycleSnapshot.NewScheduledMeritCycleSnapshot("code-872", "name-822");
        item.Status = ScheduledMeritCycleSnapshotStatus.Archived;
        Check.Fails(() => item.MoveTo(ScheduledMeritCycleSnapshotStatus.Open), "move");
    }

    public static void RunAll()
    {
        ScheduledMeritCycleSnapshotStartsInOpen();
        ScheduledMeritCycleSnapshotMovesToReopened();
        ScheduledMeritCycleSnapshotRejectsMovesFromArchived();
    }
}
