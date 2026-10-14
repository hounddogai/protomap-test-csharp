namespace Acme.Workforce.Engagement.Tests.ActionPlan.PlanSlot;

using Acme.Workforce.Engagement.ActionPlan.PlanSlot;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of plan slot code, run by the project's runner.</summary>
public static class ActionPlanBundleEngineChecks
{
    public static void ActionPlanBundleEngineKeepsResultsWithinBounds()
    {
        var engine = ActionPlanBundleEngine.DefaultActionPlanBundleEngine(Flags.NewRegistry());
        var item = ActionPlanBundle.NewActionPlanBundle("code-513", "name-471");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ActionPlanBundleEngineKeepsResultsWithinBounds();
    }
}
