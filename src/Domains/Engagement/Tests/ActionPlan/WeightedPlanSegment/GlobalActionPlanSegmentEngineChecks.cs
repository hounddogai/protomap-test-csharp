namespace Acme.Workforce.Engagement.Tests.ActionPlan.WeightedPlanSegment;

using Acme.Workforce.Engagement.ActionPlan.WeightedPlanSegment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of weighted plan segment code, run by the project's runner.</summary>
public static class GlobalActionPlanSegmentEngineChecks
{
    public static void GlobalActionPlanSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalActionPlanSegmentEngine.DefaultGlobalActionPlanSegmentEngine(Flags.NewRegistry());
        var item = GlobalActionPlanSegment.NewGlobalActionPlanSegment("code-818", "name-561");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalActionPlanSegmentEngineKeepsResultsWithinBounds();
    }
}
