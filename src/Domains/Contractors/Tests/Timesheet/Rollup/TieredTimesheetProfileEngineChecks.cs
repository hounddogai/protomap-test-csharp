namespace Acme.Workforce.Contractors.Tests.Timesheet.Rollup;

using Acme.Workforce.Contractors.Timesheet.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class TieredTimesheetProfileEngineChecks
{
    public static void TieredTimesheetProfileEngineKeepsResultsWithinBounds()
    {
        var engine = TieredTimesheetProfileEngine.DefaultTieredTimesheetProfileEngine();
        var item = TieredTimesheetProfile.NewTieredTimesheetProfile("code-298", "title-477");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredTimesheetProfileEngineKeepsResultsWithinBounds();
    }
}
