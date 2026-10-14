namespace Acme.Workforce.Assets.Tests.Refresh.Rollup;

using Acme.Workforce.Assets.Refresh.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class RefreshScoreEngineChecks
{
    public static void RefreshScoreEngineKeepsResultsWithinBounds()
    {
        var engine = RefreshScoreEngine.DefaultRefreshScoreEngine();
        var item = RefreshScore.NewRefreshScore("code-57", "name-499");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RefreshScoreEngineKeepsResultsWithinBounds();
    }
}
