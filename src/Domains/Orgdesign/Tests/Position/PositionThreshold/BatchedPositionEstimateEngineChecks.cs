namespace Acme.Workforce.Orgdesign.Tests.Position.PositionThreshold;

using Acme.Workforce.Orgdesign.Position.PositionThreshold;

/// <summary>Checks of position threshold code, run by the project's runner.</summary>
public static class BatchedPositionEstimateEngineChecks
{
    public static void BatchedPositionEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedPositionEstimateEngine.DefaultBatchedPositionEstimateEngine();
        var item = BatchedPositionEstimate.NewBatchedPositionEstimate("code-742", "name-928");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedPositionEstimateEngineKeepsResultsWithinBounds();
    }
}
