namespace Acme.Workforce.Compensation.Tests.Adjustment.Segment;

using Acme.Workforce.Compensation.Adjustment.Segment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AdjustmentSummaryEngineChecks
{
    public static void AdjustmentSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = AdjustmentSummaryEngine.DefaultAdjustmentSummaryEngine(Flags.NewRegistry());
        var item = AdjustmentSummary.NewAdjustmentSummary("code-90", "label-148");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdjustmentSummaryEngineKeepsResultsWithinBounds();
    }
}
