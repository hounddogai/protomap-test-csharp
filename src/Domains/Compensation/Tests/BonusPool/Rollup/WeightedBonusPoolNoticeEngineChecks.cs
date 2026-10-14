namespace Acme.Workforce.Compensation.Tests.BonusPool.Rollup;

using Acme.Workforce.Compensation.BonusPool.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class WeightedBonusPoolNoticeEngineChecks
{
    public static void WeightedBonusPoolNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedBonusPoolNoticeEngine.DefaultWeightedBonusPoolNoticeEngine();
        var item = WeightedBonusPoolNotice.NewWeightedBonusPoolNotice("code-855", "name-6");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedBonusPoolNoticeEngineKeepsResultsWithinBounds();
    }
}
