namespace Acme.Workforce.Compensation.Tests.Adjustment.Request;

using Acme.Workforce.Compensation.Adjustment.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class LegacyAdjustmentBatchEngineChecks
{
    public static void LegacyAdjustmentBatchEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyAdjustmentBatchEngine.DefaultLegacyAdjustmentBatchEngine();
        var item = LegacyAdjustmentBatch.NewLegacyAdjustmentBatch("code-375", "title-283");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyAdjustmentBatchEngineKeepsResultsWithinBounds();
    }
}
