namespace Acme.Workforce.Contractors.Tests.Rate.RateBatch;

using Acme.Workforce.Contractors.Rate.RateBatch;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of rate batch code, run by the project's runner.</summary>
public static class WeightedRateBatchEngineChecks
{
    public static void WeightedRateBatchEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedRateBatchEngine.DefaultWeightedRateBatchEngine(Flags.NewRegistry());
        var item = WeightedRateBatch.NewWeightedRateBatch("code-229", "title-480");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedRateBatchEngineKeepsResultsWithinBounds();
    }
}
