namespace Acme.Workforce.Contractors.Tests.Rate.RateCycle;

using Acme.Workforce.Contractors.Rate.RateCycle;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of rate cycle code, run by the project's runner.</summary>
public static class FallbackRateCycleEngineChecks
{
    public static void FallbackRateCycleEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackRateCycleEngine.DefaultFallbackRateCycleEngine(Flags.NewRegistry());
        var item = FallbackRateCycle.NewFallbackRateCycle("code-623", "name-244");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackRateCycleEngineKeepsResultsWithinBounds();
    }
}
