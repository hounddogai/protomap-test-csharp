namespace Acme.Workforce.Compensation.Tests.Adjustment.Bundle;

using Acme.Workforce.Compensation.Adjustment.Bundle;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class GlobalAdjustmentBundleEngineChecks
{
    public static void GlobalAdjustmentBundleEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalAdjustmentBundleEngine.DefaultGlobalAdjustmentBundleEngine(Flags.NewRegistry());
        var item = GlobalAdjustmentBundle.NewGlobalAdjustmentBundle("code-732", "name-226");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalAdjustmentBundleEngineKeepsResultsWithinBounds();
    }
}
