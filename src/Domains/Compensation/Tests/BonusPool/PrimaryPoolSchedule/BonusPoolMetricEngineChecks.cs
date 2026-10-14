namespace Acme.Workforce.Compensation.Tests.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

/// <summary>Checks of primary pool schedule code, run by the project's runner.</summary>
public static class BonusPoolMetricEngineChecks
{
    public static void BonusPoolMetricEngineKeepsResultsWithinBounds()
    {
        var engine = BonusPoolMetricEngine.DefaultBonusPoolMetricEngine();
        var item = BonusPoolMetric.NewBonusPoolMetric("code-837", "label-775");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BonusPoolMetricEngineKeepsResultsWithinBounds();
    }
}
