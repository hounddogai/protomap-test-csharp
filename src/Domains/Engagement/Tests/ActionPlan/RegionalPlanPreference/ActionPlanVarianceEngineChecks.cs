namespace Acme.Workforce.Engagement.Tests.ActionPlan.RegionalPlanPreference;

using Acme.Workforce.Engagement.ActionPlan.RegionalPlanPreference;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of regional plan preference code, run by the project's runner.</summary>
public static class ActionPlanVarianceEngineChecks
{
    public static void ActionPlanVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = ActionPlanVarianceEngine.DefaultActionPlanVarianceEngine(Flags.NewRegistry());
        var item = ActionPlanVariance.NewActionPlanVariance("code-227", "label-407");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ActionPlanVarianceEngineKeepsResultsWithinBounds();
    }
}
