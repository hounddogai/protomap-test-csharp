namespace Acme.Workforce.Compensation.Tests.BonusPool.Tier;

using Acme.Workforce.Compensation.BonusPool.Tier;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class BonusPoolTierEngineChecks
{
    public static void BonusPoolTierEngineKeepsResultsWithinBounds()
    {
        var engine = BonusPoolTierEngine.DefaultBonusPoolTierEngine(Flags.NewRegistry());
        var item = BonusPoolTier.NewBonusPoolTier("code-715", "label-938");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BonusPoolTierEngineKeepsResultsWithinBounds();
    }
}
