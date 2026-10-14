namespace Acme.Workforce.Engagement.Tests.ActionPlan.WeightedPlanSegment;

using Acme.Workforce.Engagement.ActionPlan.WeightedPlanSegment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of weighted plan segment code, run by the project's runner.</summary>
public static class GlobalActionPlanEntryEngineChecks
{
    public static void GlobalActionPlanEntryEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalActionPlanEntryEngine.DefaultGlobalActionPlanEntryEngine(Flags.NewRegistry());
        var item = GlobalActionPlanEntry.NewGlobalActionPlanEntry("code-111", "title-153");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalActionPlanEntryEngineKeepsResultsWithinBounds();
    }
}
