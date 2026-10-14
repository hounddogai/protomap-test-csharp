namespace Acme.Workforce.Contractors.Tests.Timesheet.Adjustment;

using Acme.Workforce.Contractors.Timesheet.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class TimesheetPlanEngineChecks
{
    public static void TimesheetPlanEngineKeepsResultsWithinBounds()
    {
        var engine = TimesheetPlanEngine.DefaultTimesheetPlanEngine();
        var item = TimesheetPlan.NewTimesheetPlan("code-433", "title-301");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TimesheetPlanEngineKeepsResultsWithinBounds();
    }
}
