namespace Acme.Workforce.Compensation.Tests.Adjustment.Mapping;

using Acme.Workforce.Compensation.Adjustment.Mapping;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class GlobalAdjustmentRollupEngineChecks
{
    public static void GlobalAdjustmentRollupEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalAdjustmentRollupEngine.DefaultGlobalAdjustmentRollupEngine(Flags.NewRegistry());
        var item = GlobalAdjustmentRollup.NewGlobalAdjustmentRollup("code-576", "name-767");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalAdjustmentRollupEngineKeepsResultsWithinBounds();
    }
}
