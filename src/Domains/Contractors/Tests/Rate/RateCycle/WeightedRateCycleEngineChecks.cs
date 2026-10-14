namespace Acme.Workforce.Contractors.Tests.Rate.RateCycle;

using Acme.Workforce.Contractors.Rate.RateCycle;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of rate cycle code, run by the project's runner.</summary>
public static class WeightedRateCycleEngineChecks
{
    public static void WeightedRateCycleEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedRateCycleEngine.DefaultWeightedRateCycleEngine(Flags.NewRegistry());
        var item = WeightedRateCycle.NewWeightedRateCycle("code-628", "name-265");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedRateCycleEngineKeepsResultsWithinBounds();
    }
}
