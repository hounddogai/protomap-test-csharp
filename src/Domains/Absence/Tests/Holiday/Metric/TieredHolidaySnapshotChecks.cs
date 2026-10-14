namespace Acme.Workforce.Absence.Tests.Holiday.Metric;

using Acme.Workforce.Absence.Holiday.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class TieredHolidaySnapshotChecks
{
    public static void TieredHolidaySnapshotStartsInIdle()
    {
        var item = TieredHolidaySnapshot.NewTieredHolidaySnapshot("code-378", "name-497");
        Check.Equal(item.Status, TieredHolidaySnapshotStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredHolidaySnapshotMovesToWarming()
    {
        var item = TieredHolidaySnapshot.NewTieredHolidaySnapshot("code-378", "name-497");
        try
        {
            item.MoveTo(TieredHolidaySnapshotStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredHolidaySnapshotStatus.Warming, "status");
    }

    public static void TieredHolidaySnapshotRejectsMovesFromStopped()
    {
        var item = TieredHolidaySnapshot.NewTieredHolidaySnapshot("code-378", "name-497");
        item.Status = TieredHolidaySnapshotStatus.Stopped;
        Check.Fails(() => item.MoveTo(TieredHolidaySnapshotStatus.Idle), "move");
    }

    public static void RunAll()
    {
        TieredHolidaySnapshotStartsInIdle();
        TieredHolidaySnapshotMovesToWarming();
        TieredHolidaySnapshotRejectsMovesFromStopped();
    }
}
