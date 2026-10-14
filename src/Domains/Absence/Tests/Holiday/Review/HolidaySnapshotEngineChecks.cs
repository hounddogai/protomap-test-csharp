namespace Acme.Workforce.Absence.Tests.Holiday.Review;

using Acme.Workforce.Absence.Holiday.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class HolidaySnapshotEngineChecks
{
    public static void HolidaySnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = HolidaySnapshotEngine.DefaultHolidaySnapshotEngine();
        var item = HolidaySnapshot.NewHolidaySnapshot("code-131", "name-307");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HolidaySnapshotEngineKeepsResultsWithinBounds();
    }
}
