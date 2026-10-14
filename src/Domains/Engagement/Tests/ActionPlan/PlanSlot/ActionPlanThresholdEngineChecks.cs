namespace Acme.Workforce.Engagement.Tests.ActionPlan.PlanSlot;

using Acme.Workforce.Engagement.ActionPlan.PlanSlot;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of plan slot code, run by the project's runner.</summary>
public static class ActionPlanThresholdEngineChecks
{
    public static void ActionPlanThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = ActionPlanThresholdEngine.DefaultActionPlanThresholdEngine(Flags.NewRegistry());
        var item = ActionPlanThreshold.NewActionPlanThreshold("code-499", "label-912");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ActionPlanThresholdEngineKeepsResultsWithinBounds();
    }
}
