namespace Acme.Workforce.Contractors.Tests.Rate.Score;

using Acme.Workforce.Contractors.Rate.Score;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class StrictRateForecastEngineChecks
{
    public static void StrictRateForecastEngineKeepsResultsWithinBounds()
    {
        var engine = StrictRateForecastEngine.DefaultStrictRateForecastEngine(Flags.NewRegistry());
        var item = StrictRateForecast.NewStrictRateForecast("code-924", "title-332");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictRateForecastEngineKeepsResultsWithinBounds();
    }
}
