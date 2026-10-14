namespace Acme.Workforce.Contractors.Tests.Timesheet.RecordItem;

using Acme.Workforce.Contractors.Timesheet.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class TimesheetRecordEngineChecks
{
    public static void TimesheetRecordEngineKeepsResultsWithinBounds()
    {
        var engine = TimesheetRecordEngine.DefaultTimesheetRecordEngine();
        var item = TimesheetRecord.NewTimesheetRecord("code-198", "name-432");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TimesheetRecordEngineKeepsResultsWithinBounds();
    }
}
