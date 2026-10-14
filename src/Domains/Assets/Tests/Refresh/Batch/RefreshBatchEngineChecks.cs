namespace Acme.Workforce.Assets.Tests.Refresh.Batch;

using Acme.Workforce.Assets.Refresh.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class RefreshBatchEngineChecks
{
    public static void RefreshBatchEngineKeepsResultsWithinBounds()
    {
        var engine = RefreshBatchEngine.DefaultRefreshBatchEngine();
        var item = RefreshBatch.NewRefreshBatch("code-366", "label-821");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RefreshBatchEngineKeepsResultsWithinBounds();
    }
}
