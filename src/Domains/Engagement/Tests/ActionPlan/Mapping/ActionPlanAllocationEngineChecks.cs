namespace Acme.Workforce.Engagement.Tests.ActionPlan.Mapping;

using Acme.Workforce.Engagement.ActionPlan.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class ActionPlanAllocationEngineChecks
{
    public static void ActionPlanAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = ActionPlanAllocationEngine.DefaultActionPlanAllocationEngine();
        var item = ActionPlanAllocation.NewActionPlanAllocation("code-546", "label-956");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ActionPlanAllocationEngineKeepsResultsWithinBounds();
    }
}
