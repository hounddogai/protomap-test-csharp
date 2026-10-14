namespace Acme.Workforce.Compensation.Tests.Adjustment.Notice;

using Acme.Workforce.Compensation.Adjustment.Notice;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class AdjustmentPreferenceEngineChecks
{
    public static void AdjustmentPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = AdjustmentPreferenceEngine.DefaultAdjustmentPreferenceEngine(Flags.NewRegistry());
        var item = AdjustmentPreference.NewAdjustmentPreference("code-452", "label-944");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdjustmentPreferenceEngineKeepsResultsWithinBounds();
    }
}
