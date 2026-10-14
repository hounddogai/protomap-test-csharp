namespace Acme.Workforce.Payrollops.Tests.Garnishment.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Garnishment.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class ScheduledGarnishmentPlanEngineChecks
{
    public static void ScheduledGarnishmentPlanEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledGarnishmentPlanEngine.DefaultScheduledGarnishmentPlanEngine(Flags.NewRegistry());
        var item = ScheduledGarnishmentPlan.NewScheduledGarnishmentPlan("code-702", "title-667");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledGarnishmentPlanEngineKeepsResultsWithinBounds();
    }
}
