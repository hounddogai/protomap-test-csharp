namespace Acme.Workforce.Orgdesign.Tests.Headcount.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Headcount.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class HeadcountForecastEngineChecks
{
    public static void HeadcountForecastEngineKeepsResultsWithinBounds()
    {
        var engine = HeadcountForecastEngine.DefaultHeadcountForecastEngine(Flags.NewRegistry());
        var item = HeadcountForecast.NewHeadcountForecast("code-171", "name-528");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HeadcountForecastEngineKeepsResultsWithinBounds();
    }
}
