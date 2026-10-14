namespace Acme.Workforce.Compensation.Tests.BonusPool.Rollup;

using Acme.Workforce.Compensation.BonusPool.Rollup;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class BonusPoolRollupEngineChecks
{
    public static void BonusPoolRollupEngineKeepsResultsWithinBounds()
    {
        var engine = BonusPoolRollupEngine.DefaultBonusPoolRollupEngine(Flags.NewRegistry());
        var item = BonusPoolRollup.NewBonusPoolRollup("code-533", "name-818");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BonusPoolRollupEngineKeepsResultsWithinBounds();
    }
}
