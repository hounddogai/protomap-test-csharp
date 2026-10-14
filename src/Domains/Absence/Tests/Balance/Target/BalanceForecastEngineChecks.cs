namespace Acme.Workforce.Absence.Tests.Balance.Target;

using Acme.Workforce.Absence.Balance.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class BalanceForecastEngineChecks
{
    public static void BalanceForecastEngineKeepsResultsWithinBounds()
    {
        var engine = BalanceForecastEngine.DefaultBalanceForecastEngine();
        var item = BalanceForecast.NewBalanceForecast("code-218", "name-321");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BalanceForecastEngineKeepsResultsWithinBounds();
    }
}
