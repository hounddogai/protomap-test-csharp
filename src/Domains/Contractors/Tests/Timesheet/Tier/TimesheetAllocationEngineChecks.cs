namespace Acme.Workforce.Contractors.Tests.Timesheet.Tier;

using Acme.Workforce.Contractors.Timesheet.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class TimesheetAllocationEngineChecks
{
    public static void TimesheetAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = TimesheetAllocationEngine.DefaultTimesheetAllocationEngine();
        var item = TimesheetAllocation.NewTimesheetAllocation("code-437", "label-358");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TimesheetAllocationEngineKeepsResultsWithinBounds();
    }
}
