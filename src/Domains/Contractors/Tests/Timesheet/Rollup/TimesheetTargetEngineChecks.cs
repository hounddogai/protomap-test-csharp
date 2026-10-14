namespace Acme.Workforce.Contractors.Tests.Timesheet.Rollup;

using Acme.Workforce.Contractors.Timesheet.Rollup;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class TimesheetTargetEngineChecks
{
    public static void TimesheetTargetEngineKeepsResultsWithinBounds()
    {
        var engine = TimesheetTargetEngine.DefaultTimesheetTargetEngine(Flags.NewRegistry());
        var item = TimesheetTarget.NewTimesheetTarget("code-951", "label-864");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TimesheetTargetEngineKeepsResultsWithinBounds();
    }
}
