namespace Acme.Workforce.Contractors.Tests.Timesheet.OverrideItem;

using Acme.Workforce.Contractors.Timesheet.OverrideItem;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class TimesheetOverrideEngineChecks
{
    public static void TimesheetOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = TimesheetOverrideEngine.DefaultTimesheetOverrideEngine(Flags.NewRegistry());
        var item = TimesheetOverride.NewTimesheetOverride("code-410", "label-946");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TimesheetOverrideEngineKeepsResultsWithinBounds();
    }
}
