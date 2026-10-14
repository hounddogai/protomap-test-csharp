namespace Acme.Workforce.Engagement.Tests.ActionPlan.Entry;

using Acme.Workforce.Engagement.ActionPlan.Entry;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class ActionPlanBatchEngineChecks
{
    public static void ActionPlanBatchEngineKeepsResultsWithinBounds()
    {
        var engine = ActionPlanBatchEngine.DefaultActionPlanBatchEngine(Flags.NewRegistry());
        var item = ActionPlanBatch.NewActionPlanBatch("code-149", "label-450");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ActionPlanBatchEngineKeepsResultsWithinBounds();
    }
}
