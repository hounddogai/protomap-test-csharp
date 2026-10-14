namespace Acme.Workforce.Contractors.Tests.Timesheet.TimesheetEstimate;

using Acme.Workforce.Contractors.Timesheet.TimesheetEstimate;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of timesheet estimate code, run by the project's runner.</summary>
public static class LegacyTimesheetEstimateEngineChecks
{
    public static void LegacyTimesheetEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyTimesheetEstimateEngine.DefaultLegacyTimesheetEstimateEngine(Flags.NewRegistry());
        var item = LegacyTimesheetEstimate.NewLegacyTimesheetEstimate("code-889", "title-474");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyTimesheetEstimateEngineKeepsResultsWithinBounds();
    }
}
