namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolSchedule;

using Acme.Workforce.Compensation.BonusPool.PoolSchedule;

/// <summary>Checks of pool schedule code, run by the project's runner.</summary>
public static class BonusPoolScoreEngineChecks
{
    public static void BonusPoolScoreEngineKeepsResultsWithinBounds()
    {
        var engine = BonusPoolScoreEngine.DefaultBonusPoolScoreEngine();
        var item = BonusPoolScore.NewBonusPoolScore("code-228", "title-382");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BonusPoolScoreEngineKeepsResultsWithinBounds();
    }
}
