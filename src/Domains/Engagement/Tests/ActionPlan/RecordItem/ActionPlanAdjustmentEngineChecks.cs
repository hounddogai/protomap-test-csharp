namespace Acme.Workforce.Engagement.Tests.ActionPlan.RecordItem;

using Acme.Workforce.Engagement.ActionPlan.RecordItem;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class ActionPlanAdjustmentEngineChecks
{
    public static void ActionPlanAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = ActionPlanAdjustmentEngine.DefaultActionPlanAdjustmentEngine(Flags.NewRegistry());
        var item = ActionPlanAdjustment.NewActionPlanAdjustment("code-766", "title-665");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ActionPlanAdjustmentEngineKeepsResultsWithinBounds();
    }
}
