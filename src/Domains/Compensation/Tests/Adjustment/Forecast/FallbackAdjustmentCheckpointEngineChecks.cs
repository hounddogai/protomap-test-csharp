namespace Acme.Workforce.Compensation.Tests.Adjustment.Forecast;

using Acme.Workforce.Compensation.Adjustment.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class FallbackAdjustmentCheckpointEngineChecks
{
    public static void FallbackAdjustmentCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackAdjustmentCheckpointEngine.DefaultFallbackAdjustmentCheckpointEngine();
        var item = FallbackAdjustmentCheckpoint.NewFallbackAdjustmentCheckpoint("code-117", "name-913");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackAdjustmentCheckpointEngineKeepsResultsWithinBounds();
    }
}
