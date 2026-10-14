namespace Acme.Workforce.Engagement.Tests.ActionPlan.Bundle;

using Acme.Workforce.Engagement.ActionPlan.Bundle;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class ActionPlanItemEngineChecks
{
    public static void ActionPlanItemEngineKeepsResultsWithinBounds()
    {
        var engine = ActionPlanItemEngine.DefaultActionPlanItemEngine(Flags.NewRegistry());
        var item = ActionPlanItem.NewActionPlanItem("code-818", "name-425");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ActionPlanItemEngineKeepsResultsWithinBounds();
    }
}
