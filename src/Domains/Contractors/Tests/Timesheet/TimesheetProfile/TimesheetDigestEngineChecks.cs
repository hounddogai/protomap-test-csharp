namespace Acme.Workforce.Contractors.Tests.Timesheet.TimesheetProfile;

using Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

/// <summary>Checks of timesheet profile code, run by the project's runner.</summary>
public static class TimesheetDigestEngineChecks
{
    public static void TimesheetDigestEngineKeepsResultsWithinBounds()
    {
        var engine = TimesheetDigestEngine.DefaultTimesheetDigestEngine();
        var item = TimesheetDigest.NewTimesheetDigest("code-894", "label-86");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TimesheetDigestEngineKeepsResultsWithinBounds();
    }
}
