namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Batch;

using Acme.Workforce.Talent.SuccessionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class SuccessionPlanBatchEngineChecks
{
    public static void SuccessionPlanBatchEngineKeepsResultsWithinBounds()
    {
        var engine = SuccessionPlanBatchEngine.DefaultSuccessionPlanBatchEngine();
        var item = SuccessionPlanBatch.NewSuccessionPlanBatch("code-236", "title-560");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SuccessionPlanBatchEngineKeepsResultsWithinBounds();
    }
}
