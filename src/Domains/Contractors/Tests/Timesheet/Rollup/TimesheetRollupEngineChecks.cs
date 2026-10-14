namespace Acme.Workforce.Contractors.Tests.Timesheet.Rollup;

using Acme.Workforce.Contractors.Timesheet.Rollup;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class TimesheetRollupEngineChecks
{
    public static void TimesheetRollupEngineKeepsResultsWithinBounds()
    {
        var engine = TimesheetRollupEngine.DefaultTimesheetRollupEngine(Flags.NewRegistry());
        var item = TimesheetRollup.NewTimesheetRollup("code-379", "label-419");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TimesheetRollupEngineKeepsResultsWithinBounds();
    }
}
