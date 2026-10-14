namespace Acme.Workforce.Contractors.Tests.Timesheet.Group;

using Acme.Workforce.Contractors.Timesheet.Group;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class RegionalTimesheetProfileEngineChecks
{
    public static void RegionalTimesheetProfileEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalTimesheetProfileEngine.DefaultRegionalTimesheetProfileEngine(Flags.NewRegistry());
        var item = RegionalTimesheetProfile.NewRegionalTimesheetProfile("code-824", "name-595");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalTimesheetProfileEngineKeepsResultsWithinBounds();
    }
}
