namespace Acme.Workforce.Engagement.Tests.ActionPlan.PlanSlot;

using Acme.Workforce.Engagement.ActionPlan.PlanSlot;

/// <summary>Checks of plan slot code, run by the project's runner.</summary>
public static class ManualActionPlanSlotEngineChecks
{
    public static void ManualActionPlanSlotEngineKeepsResultsWithinBounds()
    {
        var engine = ManualActionPlanSlotEngine.DefaultManualActionPlanSlotEngine();
        var item = ManualActionPlanSlot.NewManualActionPlanSlot("code-303", "name-195");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualActionPlanSlotEngineKeepsResultsWithinBounds();
    }
}
