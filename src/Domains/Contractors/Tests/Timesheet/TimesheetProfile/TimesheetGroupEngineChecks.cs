namespace Acme.Workforce.Contractors.Tests.Timesheet.TimesheetProfile;

using Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

/// <summary>Checks of timesheet profile code, run by the project's runner.</summary>
public static class TimesheetGroupEngineChecks
{
    public static void TimesheetGroupEngineKeepsResultsWithinBounds()
    {
        var engine = TimesheetGroupEngine.DefaultTimesheetGroupEngine();
        var item = TimesheetGroup.NewTimesheetGroup("code-131", "title-654");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TimesheetGroupEngineKeepsResultsWithinBounds();
    }
}
