namespace Acme.Workforce.Assets.Tests.Laptop.Review;

using Acme.Workforce.Assets.Laptop.Review;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class BatchedLaptopVarianceEngineChecks
{
    public static void BatchedLaptopVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedLaptopVarianceEngine.DefaultBatchedLaptopVarianceEngine(Flags.NewRegistry());
        var item = BatchedLaptopVariance.NewBatchedLaptopVariance("code-421", "title-903");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedLaptopVarianceEngineKeepsResultsWithinBounds();
    }
}
