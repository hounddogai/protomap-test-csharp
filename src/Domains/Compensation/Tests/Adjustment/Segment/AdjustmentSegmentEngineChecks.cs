namespace Acme.Workforce.Compensation.Tests.Adjustment.Segment;

using Acme.Workforce.Compensation.Adjustment.Segment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AdjustmentSegmentEngineChecks
{
    public static void AdjustmentSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = AdjustmentSegmentEngine.DefaultAdjustmentSegmentEngine(Flags.NewRegistry());
        var item = AdjustmentSegment.NewAdjustmentSegment("code-150", "label-84");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdjustmentSegmentEngineKeepsResultsWithinBounds();
    }
}
