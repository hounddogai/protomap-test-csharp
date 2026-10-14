namespace Acme.Workforce.Talent.Tests.Mobility.Forecast;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Mobility.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class MobilityForecastEngineChecks
{
    public static void MobilityForecastEngineKeepsResultsWithinBounds()
    {
        var engine = MobilityForecastEngine.DefaultMobilityForecastEngine(Flags.NewRegistry());
        var item = MobilityForecast.NewMobilityForecast("code-269", "name-404");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MobilityForecastEngineKeepsResultsWithinBounds();
    }
}
