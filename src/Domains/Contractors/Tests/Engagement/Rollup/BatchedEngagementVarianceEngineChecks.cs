namespace Acme.Workforce.Contractors.Tests.Engagement.Rollup;

using Acme.Workforce.Contractors.Engagement.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class BatchedEngagementVarianceEngineChecks
{
    public static void BatchedEngagementVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedEngagementVarianceEngine.DefaultBatchedEngagementVarianceEngine();
        var item = BatchedEngagementVariance.NewBatchedEngagementVariance("code-615", "label-636");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedEngagementVarianceEngineKeepsResultsWithinBounds();
    }
}
