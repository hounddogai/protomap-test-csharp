namespace Acme.Workforce.Engagement.Tests.ActionPlan.Ledger;

using Acme.Workforce.Engagement.ActionPlan.Ledger;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class LegacyActionPlanMetricEngineChecks
{
    public static void LegacyActionPlanMetricEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyActionPlanMetricEngine.DefaultLegacyActionPlanMetricEngine(Flags.NewRegistry());
        var item = LegacyActionPlanMetric.NewLegacyActionPlanMetric("code-891", "label-979");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyActionPlanMetricEngineKeepsResultsWithinBounds();
    }
}
