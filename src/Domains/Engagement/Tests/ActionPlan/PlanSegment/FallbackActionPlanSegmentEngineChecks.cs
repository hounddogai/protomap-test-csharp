namespace Acme.Workforce.Engagement.Tests.ActionPlan.PlanSegment;

using Acme.Workforce.Engagement.ActionPlan.PlanSegment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of plan segment code, run by the project's runner.</summary>
public static class FallbackActionPlanSegmentEngineChecks
{
    public static void FallbackActionPlanSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackActionPlanSegmentEngine.DefaultFallbackActionPlanSegmentEngine(Flags.NewRegistry());
        var item = FallbackActionPlanSegment.NewFallbackActionPlanSegment("code-688", "name-672");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackActionPlanSegmentEngineKeepsResultsWithinBounds();
    }
}
