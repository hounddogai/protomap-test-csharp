namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolTier;

using Acme.Workforce.Compensation.BonusPool.PoolTier;

/// <summary>Checks of pool tier code, run by the project's runner.</summary>
public static class PrimaryBonusPoolTierEngineChecks
{
    public static void PrimaryBonusPoolTierEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryBonusPoolTierEngine.DefaultPrimaryBonusPoolTierEngine();
        var item = PrimaryBonusPoolTier.NewPrimaryBonusPoolTier("code-572", "title-366");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryBonusPoolTierEngineKeepsResultsWithinBounds();
    }
}
