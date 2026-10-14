namespace Acme.Workforce.Engagement.Tests.ActionPlan.Slot;

using Acme.Workforce.Engagement.ActionPlan.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class ActionPlanSlotEngineChecks
{
    public static void ActionPlanSlotEngineKeepsResultsWithinBounds()
    {
        var engine = ActionPlanSlotEngine.DefaultActionPlanSlotEngine();
        var item = ActionPlanSlot.NewActionPlanSlot("code-833", "name-304");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ActionPlanSlotEngineKeepsResultsWithinBounds();
    }
}
