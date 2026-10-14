namespace Acme.Workforce.Assets.Tests.Refresh.Rollup;

using Acme.Workforce.Assets.Refresh.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class RefreshRequestEngineChecks
{
    public static void RefreshRequestEngineKeepsResultsWithinBounds()
    {
        var engine = RefreshRequestEngine.DefaultRefreshRequestEngine();
        var item = RefreshRequest.NewRefreshRequest("code-482", "title-631");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RefreshRequestEngineKeepsResultsWithinBounds();
    }
}
