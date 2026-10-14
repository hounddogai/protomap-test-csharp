namespace Acme.Workforce.Contractors.Tests.Timesheet.Summary;

using Acme.Workforce.Contractors.Timesheet.Summary;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class TimesheetSummaryEngineChecks
{
    public static void TimesheetSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = TimesheetSummaryEngine.DefaultTimesheetSummaryEngine(Flags.NewRegistry());
        var item = TimesheetSummary.NewTimesheetSummary("code-541", "name-859");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TimesheetSummaryEngineKeepsResultsWithinBounds();
    }
}
