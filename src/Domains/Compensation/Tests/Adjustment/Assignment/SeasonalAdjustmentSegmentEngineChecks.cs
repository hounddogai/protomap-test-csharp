namespace Acme.Workforce.Compensation.Tests.Adjustment.Assignment;

using Acme.Workforce.Compensation.Adjustment.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class SeasonalAdjustmentSegmentEngineChecks
{
    public static void SeasonalAdjustmentSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalAdjustmentSegmentEngine.DefaultSeasonalAdjustmentSegmentEngine();
        var item = SeasonalAdjustmentSegment.NewSeasonalAdjustmentSegment("code-873", "label-713");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalAdjustmentSegmentEngineKeepsResultsWithinBounds();
    }
}
