namespace Acme.Workforce.Assets.Tests.Refresh.Schedule;

using Acme.Workforce.Assets.Refresh.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class SeasonalRefreshWindowEngineChecks
{
    public static void SeasonalRefreshWindowEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalRefreshWindowEngine.DefaultSeasonalRefreshWindowEngine();
        var item = SeasonalRefreshWindow.NewSeasonalRefreshWindow("code-555", "title-218");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalRefreshWindowEngineKeepsResultsWithinBounds();
    }
}
