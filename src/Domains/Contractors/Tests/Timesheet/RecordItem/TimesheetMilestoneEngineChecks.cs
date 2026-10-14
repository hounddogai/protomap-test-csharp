namespace Acme.Workforce.Contractors.Tests.Timesheet.RecordItem;

using Acme.Workforce.Contractors.Timesheet.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class TimesheetMilestoneEngineChecks
{
    public static void TimesheetMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = TimesheetMilestoneEngine.DefaultTimesheetMilestoneEngine();
        var item = TimesheetMilestone.NewTimesheetMilestone("code-59", "name-458");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TimesheetMilestoneEngineKeepsResultsWithinBounds();
    }
}
