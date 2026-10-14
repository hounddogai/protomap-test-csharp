namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolTier;

using Acme.Workforce.Compensation.BonusPool.PoolTier;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of pool tier code, run by the project's runner.</summary>
public static class LegacyBonusPoolAllocationEngineChecks
{
    public static void LegacyBonusPoolAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyBonusPoolAllocationEngine.DefaultLegacyBonusPoolAllocationEngine(Flags.NewRegistry());
        var item = LegacyBonusPoolAllocation.NewLegacyBonusPoolAllocation("code-296", "title-345");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyBonusPoolAllocationEngineKeepsResultsWithinBounds();
    }
}
