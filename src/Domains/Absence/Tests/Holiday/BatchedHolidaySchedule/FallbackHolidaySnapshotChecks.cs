namespace Acme.Workforce.Absence.Tests.Holiday.BatchedHolidaySchedule;

using Acme.Workforce.Absence.Holiday.BatchedHolidaySchedule;

/// <summary>Checks of batched holiday schedule code, run by the project's runner.</summary>
public static class FallbackHolidaySnapshotChecks
{
    public static void FallbackHolidaySnapshotStartsInNew()
    {
        var item = FallbackHolidaySnapshot.NewFallbackHolidaySnapshot("code-194", "label-2");
        Check.Equal(item.Status, FallbackHolidaySnapshotStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackHolidaySnapshotMovesToDone()
    {
        var item = FallbackHolidaySnapshot.NewFallbackHolidaySnapshot("code-194", "label-2");
        try
        {
            item.MoveTo(FallbackHolidaySnapshotStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackHolidaySnapshotStatus.Done, "status");
    }

    public static void FallbackHolidaySnapshotRejectsMovesFromFailed()
    {
        var item = FallbackHolidaySnapshot.NewFallbackHolidaySnapshot("code-194", "label-2");
        item.Status = FallbackHolidaySnapshotStatus.Failed;
        Check.Fails(() => item.MoveTo(FallbackHolidaySnapshotStatus.New), "move");
    }

    public static void RunAll()
    {
        FallbackHolidaySnapshotStartsInNew();
        FallbackHolidaySnapshotMovesToDone();
        FallbackHolidaySnapshotRejectsMovesFromFailed();
    }
}
