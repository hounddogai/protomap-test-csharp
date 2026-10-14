namespace Acme.Workforce.Assets.Tests.Refresh.Window;

using Acme.Workforce.Assets.Refresh.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class RegionalRefreshCycleEngineChecks
{
    public static void RegionalRefreshCycleEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalRefreshCycleEngine.DefaultRegionalRefreshCycleEngine();
        var item = RegionalRefreshCycle.NewRegionalRefreshCycle("code-199", "name-723");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalRefreshCycleEngineKeepsResultsWithinBounds();
    }
}
