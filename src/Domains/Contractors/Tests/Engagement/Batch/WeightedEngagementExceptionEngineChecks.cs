namespace Acme.Workforce.Contractors.Tests.Engagement.Batch;

using Acme.Workforce.Contractors.Engagement.Batch;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class WeightedEngagementExceptionEngineChecks
{
    public static void WeightedEngagementExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedEngagementExceptionEngine.DefaultWeightedEngagementExceptionEngine(Flags.NewRegistry());
        var item = WeightedEngagementException.NewWeightedEngagementException("code-101", "name-617");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedEngagementExceptionEngineKeepsResultsWithinBounds();
    }
}
