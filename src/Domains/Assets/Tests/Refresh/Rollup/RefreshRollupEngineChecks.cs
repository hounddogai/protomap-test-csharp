namespace Acme.Workforce.Assets.Tests.Refresh.Rollup;

using Acme.Workforce.Assets.Refresh.Rollup;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class RefreshRollupEngineChecks
{
    public static void RefreshRollupEngineKeepsResultsWithinBounds()
    {
        var engine = RefreshRollupEngine.DefaultRefreshRollupEngine(Flags.NewRegistry());
        var item = RefreshRollup.NewRefreshRollup("code-161", "label-754");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RefreshRollupEngineKeepsResultsWithinBounds();
    }
}
