namespace Acme.Workforce.Contractors.Tests.Timesheet.Policy;

using Acme.Workforce.Contractors.Timesheet.Policy;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class TimesheetSegmentEngineChecks
{
    public static void TimesheetSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = TimesheetSegmentEngine.DefaultTimesheetSegmentEngine(Flags.NewRegistry());
        var item = TimesheetSegment.NewTimesheetSegment("code-494", "name-332");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TimesheetSegmentEngineKeepsResultsWithinBounds();
    }
}
