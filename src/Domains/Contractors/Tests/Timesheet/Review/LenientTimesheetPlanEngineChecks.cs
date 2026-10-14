namespace Acme.Workforce.Contractors.Tests.Timesheet.Review;

using Acme.Workforce.Contractors.Timesheet.Review;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class LenientTimesheetPlanEngineChecks
{
    public static void LenientTimesheetPlanEngineKeepsResultsWithinBounds()
    {
        var engine = LenientTimesheetPlanEngine.DefaultLenientTimesheetPlanEngine(Flags.NewRegistry());
        var item = LenientTimesheetPlan.NewLenientTimesheetPlan("code-394", "name-992");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientTimesheetPlanEngineKeepsResultsWithinBounds();
    }
}
