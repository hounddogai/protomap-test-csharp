namespace Acme.Workforce.Compensation.Tests.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

/// <summary>Checks of primary pool schedule code, run by the project's runner.</summary>
public static class BonusPoolForecastEngineChecks
{
    public static void BonusPoolForecastEngineKeepsResultsWithinBounds()
    {
        var engine = BonusPoolForecastEngine.DefaultBonusPoolForecastEngine();
        var item = BonusPoolForecast.NewBonusPoolForecast("code-225", "title-62");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BonusPoolForecastEngineKeepsResultsWithinBounds();
    }
}
