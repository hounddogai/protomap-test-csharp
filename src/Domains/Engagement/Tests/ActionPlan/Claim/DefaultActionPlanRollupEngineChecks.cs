namespace Acme.Workforce.Engagement.Tests.ActionPlan.Claim;

using Acme.Workforce.Engagement.ActionPlan.Claim;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class DefaultActionPlanRollupEngineChecks
{
    public static void DefaultActionPlanRollupEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultActionPlanRollupEngine.DefaultDefaultActionPlanRollupEngine(Flags.NewRegistry());
        var item = DefaultActionPlanRollup.NewDefaultActionPlanRollup("code-883", "label-183");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultActionPlanRollupEngineKeepsResultsWithinBounds();
    }
}
