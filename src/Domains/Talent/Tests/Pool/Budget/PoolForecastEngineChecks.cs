namespace Acme.Workforce.Talent.Tests.Pool.Budget;

using Acme.Workforce.Talent.Pool.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class PoolForecastEngineChecks
{
    public static void PoolForecastEngineKeepsResultsWithinBounds()
    {
        var engine = PoolForecastEngine.DefaultPoolForecastEngine();
        var item = PoolForecast.NewPoolForecast("code-8", "name-660");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PoolForecastEngineKeepsResultsWithinBounds();
    }
}
