namespace Acme.Workforce.Absence.Tests.Holiday.Metric;

using Acme.Workforce.Absence.Holiday.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class TieredHolidaySnapshotEngineChecks
{
    public static void TieredHolidaySnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = TieredHolidaySnapshotEngine.DefaultTieredHolidaySnapshotEngine();
        var item = TieredHolidaySnapshot.NewTieredHolidaySnapshot("code-378", "name-497");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredHolidaySnapshotEngineKeepsResultsWithinBounds();
    }
}
