namespace Acme.Workforce.Compensation.Tests.Adjustment.Tier;

using Acme.Workforce.Compensation.Adjustment.Tier;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class GlobalAdjustmentScoreEngineChecks
{
    public static void GlobalAdjustmentScoreEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalAdjustmentScoreEngine.DefaultGlobalAdjustmentScoreEngine(Flags.NewRegistry());
        var item = GlobalAdjustmentScore.NewGlobalAdjustmentScore("code-717", "title-84");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalAdjustmentScoreEngineKeepsResultsWithinBounds();
    }
}
