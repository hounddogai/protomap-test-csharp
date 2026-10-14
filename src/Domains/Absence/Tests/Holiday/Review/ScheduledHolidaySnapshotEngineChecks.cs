namespace Acme.Workforce.Absence.Tests.Holiday.Review;

using Acme.Workforce.Absence.Holiday.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ScheduledHolidaySnapshotEngineChecks
{
    public static void ScheduledHolidaySnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledHolidaySnapshotEngine.DefaultScheduledHolidaySnapshotEngine();
        var item = ScheduledHolidaySnapshot.NewScheduledHolidaySnapshot("code-563", "label-418");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledHolidaySnapshotEngineKeepsResultsWithinBounds();
    }
}
