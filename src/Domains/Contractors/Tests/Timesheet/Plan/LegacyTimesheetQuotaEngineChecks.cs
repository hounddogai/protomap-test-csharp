namespace Acme.Workforce.Contractors.Tests.Timesheet.Plan;

using Acme.Workforce.Contractors.Timesheet.Plan;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class LegacyTimesheetQuotaEngineChecks
{
    public static void LegacyTimesheetQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyTimesheetQuotaEngine.DefaultLegacyTimesheetQuotaEngine(Flags.NewRegistry());
        var item = LegacyTimesheetQuota.NewLegacyTimesheetQuota("code-873", "title-610");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyTimesheetQuotaEngineKeepsResultsWithinBounds();
    }
}
