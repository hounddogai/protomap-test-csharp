namespace Acme.Workforce.Engagement.Tests.ActionPlan.Slot;

using Acme.Workforce.Engagement.ActionPlan.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class ActionPlanDraftEngineChecks
{
    public static void ActionPlanDraftEngineKeepsResultsWithinBounds()
    {
        var engine = ActionPlanDraftEngine.DefaultActionPlanDraftEngine();
        var item = ActionPlanDraft.NewActionPlanDraft("code-335", "label-193");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ActionPlanDraftEngineKeepsResultsWithinBounds();
    }
}
