namespace Acme.Workforce.Engagement.Tests.ActionPlan.Draft;

using Acme.Workforce.Engagement.ActionPlan.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class FallbackActionPlanDraftEngineChecks
{
    public static void FallbackActionPlanDraftEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackActionPlanDraftEngine.DefaultFallbackActionPlanDraftEngine();
        var item = FallbackActionPlanDraft.NewFallbackActionPlanDraft("code-538", "label-842");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackActionPlanDraftEngineKeepsResultsWithinBounds();
    }
}
