namespace Acme.Workforce.Compensation.Tests.Adjustment.Profile;

using Acme.Workforce.Compensation.Adjustment.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AdjustmentThresholdEngineChecks
{
    public static void AdjustmentThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = AdjustmentThresholdEngine.DefaultAdjustmentThresholdEngine();
        var item = AdjustmentThreshold.NewAdjustmentThreshold("code-889", "label-938");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdjustmentThresholdEngineKeepsResultsWithinBounds();
    }
}
