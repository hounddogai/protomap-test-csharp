namespace Acme.Workforce.Contractors.Tests.Timesheet.Plan;

using Acme.Workforce.Contractors.Timesheet.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class PrimaryTimesheetPlanEngineChecks
{
    public static void PrimaryTimesheetPlanEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryTimesheetPlanEngine.DefaultPrimaryTimesheetPlanEngine();
        var item = PrimaryTimesheetPlan.NewPrimaryTimesheetPlan("code-861", "name-569");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryTimesheetPlanEngineKeepsResultsWithinBounds();
    }
}
