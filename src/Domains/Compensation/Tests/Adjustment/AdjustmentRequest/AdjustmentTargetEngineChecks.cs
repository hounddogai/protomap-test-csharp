namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentRequest;

using Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

/// <summary>Checks of adjustment request code, run by the project's runner.</summary>
public static class AdjustmentTargetEngineChecks
{
    public static void AdjustmentTargetEngineKeepsResultsWithinBounds()
    {
        var engine = AdjustmentTargetEngine.DefaultAdjustmentTargetEngine();
        var item = AdjustmentTarget.NewAdjustmentTarget("code-12", "label-137");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdjustmentTargetEngineKeepsResultsWithinBounds();
    }
}
