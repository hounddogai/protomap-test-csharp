namespace Acme.Workforce.Engagement.Tests.ActionPlan.Metric;

using Acme.Workforce.Engagement.ActionPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class DefaultActionPlanDraftEngineChecks
{
    public static void DefaultActionPlanDraftEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultActionPlanDraftEngine.DefaultDefaultActionPlanDraftEngine();
        var item = DefaultActionPlanDraft.NewDefaultActionPlanDraft("code-767", "label-759");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultActionPlanDraftEngineKeepsResultsWithinBounds();
    }
}
