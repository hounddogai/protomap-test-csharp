namespace Acme.Workforce.Contractors.Tests.Timesheet.Preference;

using Acme.Workforce.Contractors.Timesheet.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class TimesheetWindowEngineChecks
{
    public static void TimesheetWindowEngineKeepsResultsWithinBounds()
    {
        var engine = TimesheetWindowEngine.DefaultTimesheetWindowEngine();
        var item = TimesheetWindow.NewTimesheetWindow("code-999", "title-394");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TimesheetWindowEngineKeepsResultsWithinBounds();
    }
}
