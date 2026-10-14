namespace Acme.Workforce.Contractors.Tests.Timesheet.Adjustment;

using Acme.Workforce.Contractors.Timesheet.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class TimesheetThresholdEngineChecks
{
    public static void TimesheetThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = TimesheetThresholdEngine.DefaultTimesheetThresholdEngine();
        var item = TimesheetThreshold.NewTimesheetThreshold("code-20", "title-196");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TimesheetThresholdEngineKeepsResultsWithinBounds();
    }
}
