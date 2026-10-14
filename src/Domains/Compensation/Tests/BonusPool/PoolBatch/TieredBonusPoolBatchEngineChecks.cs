namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolBatch;

using Acme.Workforce.Compensation.BonusPool.PoolBatch;

/// <summary>Checks of pool batch code, run by the project's runner.</summary>
public static class TieredBonusPoolBatchEngineChecks
{
    public static void TieredBonusPoolBatchEngineKeepsResultsWithinBounds()
    {
        var engine = TieredBonusPoolBatchEngine.DefaultTieredBonusPoolBatchEngine();
        var item = TieredBonusPoolBatch.NewTieredBonusPoolBatch("code-832", "name-910");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredBonusPoolBatchEngineKeepsResultsWithinBounds();
    }
}
