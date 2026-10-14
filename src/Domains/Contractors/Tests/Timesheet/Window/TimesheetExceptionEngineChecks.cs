namespace Acme.Workforce.Contractors.Tests.Timesheet.Window;

using Acme.Workforce.Contractors.Timesheet.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TimesheetExceptionEngineChecks
{
    public static void TimesheetExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = TimesheetExceptionEngine.DefaultTimesheetExceptionEngine();
        var item = TimesheetException.NewTimesheetException("code-160", "title-260");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TimesheetExceptionEngineKeepsResultsWithinBounds();
    }
}
