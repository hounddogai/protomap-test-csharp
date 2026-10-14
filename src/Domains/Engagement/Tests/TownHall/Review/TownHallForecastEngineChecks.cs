namespace Acme.Workforce.Engagement.Tests.TownHall.Review;

using Acme.Workforce.Engagement.TownHall.Review;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class TownHallForecastEngineChecks
{
    public static void TownHallForecastEngineKeepsResultsWithinBounds()
    {
        var engine = TownHallForecastEngine.DefaultTownHallForecastEngine(Flags.NewRegistry());
        var item = TownHallForecast.NewTownHallForecast("code-205", "title-274");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TownHallForecastEngineKeepsResultsWithinBounds();
    }
}
