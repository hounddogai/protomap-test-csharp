namespace Acme.Workforce.Engagement.Tests.ActionPlan.Entry;

using Acme.Workforce.Engagement.ActionPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class ActionPlanEntryEngineChecks
{
    public static void ActionPlanEntryEngineKeepsResultsWithinBounds()
    {
        var engine = ActionPlanEntryEngine.DefaultActionPlanEntryEngine();
        var item = ActionPlanEntry.NewActionPlanEntry("code-498", "label-673");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ActionPlanEntryEngineKeepsResultsWithinBounds();
    }
}
