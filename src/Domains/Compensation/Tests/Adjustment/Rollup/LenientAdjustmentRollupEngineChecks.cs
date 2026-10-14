namespace Acme.Workforce.Compensation.Tests.Adjustment.Rollup;

using Acme.Workforce.Compensation.Adjustment.Rollup;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class LenientAdjustmentRollupEngineChecks
{
    public static void LenientAdjustmentRollupEngineKeepsResultsWithinBounds()
    {
        var engine = LenientAdjustmentRollupEngine.DefaultLenientAdjustmentRollupEngine(Flags.NewRegistry());
        var item = LenientAdjustmentRollup.NewLenientAdjustmentRollup("code-735", "title-435");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientAdjustmentRollupEngineKeepsResultsWithinBounds();
    }
}
