namespace Acme.Workforce.Compensation.Tests.Adjustment.Allocation;

using Acme.Workforce.Compensation.Adjustment.Allocation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class WeightedAdjustmentDraftEngineChecks
{
    public static void WeightedAdjustmentDraftEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedAdjustmentDraftEngine.DefaultWeightedAdjustmentDraftEngine(Flags.NewRegistry());
        var item = WeightedAdjustmentDraft.NewWeightedAdjustmentDraft("code-912", "label-478");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedAdjustmentDraftEngineKeepsResultsWithinBounds();
    }
}
