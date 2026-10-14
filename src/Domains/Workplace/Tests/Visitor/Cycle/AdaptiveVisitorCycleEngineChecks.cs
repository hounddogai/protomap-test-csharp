namespace Acme.Workforce.Workplace.Tests.Visitor.Cycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Visitor.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AdaptiveVisitorCycleEngineChecks
{
    public static void AdaptiveVisitorCycleEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveVisitorCycleEngine.DefaultAdaptiveVisitorCycleEngine(Flags.NewRegistry());
        var item = AdaptiveVisitorCycle.NewAdaptiveVisitorCycle("code-891", "title-896");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveVisitorCycleEngineKeepsResultsWithinBounds();
    }
}
