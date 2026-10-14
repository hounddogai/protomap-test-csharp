namespace Acme.Workforce.Contractors.Tests.Rate.Cycle;

using Acme.Workforce.Contractors.Rate.Cycle;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class RateCycleEngineChecks
{
    public static void RateCycleEngineKeepsResultsWithinBounds()
    {
        var engine = RateCycleEngine.DefaultRateCycleEngine(Flags.NewRegistry());
        var item = RateCycle.NewRateCycle("code-881", "title-15");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RateCycleEngineKeepsResultsWithinBounds();
    }
}
