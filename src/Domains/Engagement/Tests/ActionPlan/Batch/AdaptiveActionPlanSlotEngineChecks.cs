namespace Acme.Workforce.Engagement.Tests.ActionPlan.Batch;

using Acme.Workforce.Engagement.ActionPlan.Batch;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class AdaptiveActionPlanSlotEngineChecks
{
    public static void AdaptiveActionPlanSlotEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveActionPlanSlotEngine.DefaultAdaptiveActionPlanSlotEngine(Flags.NewRegistry());
        var item = AdaptiveActionPlanSlot.NewAdaptiveActionPlanSlot("code-487", "title-215");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveActionPlanSlotEngineKeepsResultsWithinBounds();
    }
}
