namespace Acme.Workforce.Assets.Tests.Refresh.Estimate;

using Acme.Workforce.Assets.Refresh.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class RefreshCheckpointEngineChecks
{
    public static void RefreshCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = RefreshCheckpointEngine.DefaultRefreshCheckpointEngine();
        var item = RefreshCheckpoint.NewRefreshCheckpoint("code-711", "title-152");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RefreshCheckpointEngineKeepsResultsWithinBounds();
    }
}
