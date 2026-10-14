namespace Acme.Workforce.Contractors.Tests.Timesheet.Window;

using Acme.Workforce.Contractors.Timesheet.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class RegionalTimesheetWindowEngineChecks
{
    public static void RegionalTimesheetWindowEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalTimesheetWindowEngine.DefaultRegionalTimesheetWindowEngine();
        var item = RegionalTimesheetWindow.NewRegionalTimesheetWindow("code-776", "title-361");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalTimesheetWindowEngineKeepsResultsWithinBounds();
    }
}
