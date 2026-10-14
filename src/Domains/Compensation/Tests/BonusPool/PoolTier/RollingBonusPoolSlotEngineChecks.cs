namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolTier;

using Acme.Workforce.Compensation.BonusPool.PoolTier;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of pool tier code, run by the project's runner.</summary>
public static class RollingBonusPoolSlotEngineChecks
{
    public static void RollingBonusPoolSlotEngineKeepsResultsWithinBounds()
    {
        var engine = RollingBonusPoolSlotEngine.DefaultRollingBonusPoolSlotEngine(Flags.NewRegistry());
        var item = RollingBonusPoolSlot.NewRollingBonusPoolSlot("code-4", "title-550");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingBonusPoolSlotEngineKeepsResultsWithinBounds();
    }
}
