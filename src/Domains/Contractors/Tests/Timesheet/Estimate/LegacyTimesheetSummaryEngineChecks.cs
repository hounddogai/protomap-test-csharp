namespace Acme.Workforce.Contractors.Tests.Timesheet.Estimate;

using Acme.Workforce.Contractors.Timesheet.Estimate;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class LegacyTimesheetSummaryEngineChecks
{
    public static void LegacyTimesheetSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyTimesheetSummaryEngine.DefaultLegacyTimesheetSummaryEngine(Flags.NewRegistry());
        var item = LegacyTimesheetSummary.NewLegacyTimesheetSummary("code-294", "label-1");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyTimesheetSummaryEngineKeepsResultsWithinBounds();
    }
}
