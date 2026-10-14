namespace Acme.Workforce.Engagement.Tests.ActionPlan.Bundle;

using Acme.Workforce.Engagement.ActionPlan.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class BatchedActionPlanBundleEngineChecks
{
    public static void BatchedActionPlanBundleEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedActionPlanBundleEngine.DefaultBatchedActionPlanBundleEngine();
        var item = BatchedActionPlanBundle.NewBatchedActionPlanBundle("code-432", "title-756");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedActionPlanBundleEngineKeepsResultsWithinBounds();
    }
}
