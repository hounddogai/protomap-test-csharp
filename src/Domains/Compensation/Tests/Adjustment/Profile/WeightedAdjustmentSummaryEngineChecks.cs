namespace Acme.Workforce.Compensation.Tests.Adjustment.Profile;

using Acme.Workforce.Compensation.Adjustment.Profile;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class WeightedAdjustmentSummaryEngineChecks
{
    public static void WeightedAdjustmentSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedAdjustmentSummaryEngine.DefaultWeightedAdjustmentSummaryEngine(Flags.NewRegistry());
        var item = WeightedAdjustmentSummary.NewWeightedAdjustmentSummary("code-833", "label-490");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedAdjustmentSummaryEngineKeepsResultsWithinBounds();
    }
}
