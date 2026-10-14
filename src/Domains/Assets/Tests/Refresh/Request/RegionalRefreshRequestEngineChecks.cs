namespace Acme.Workforce.Assets.Tests.Refresh.Request;

using Acme.Workforce.Assets.Refresh.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class RegionalRefreshRequestEngineChecks
{
    public static void RegionalRefreshRequestEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalRefreshRequestEngine.DefaultRegionalRefreshRequestEngine();
        var item = RegionalRefreshRequest.NewRegionalRefreshRequest("code-620", "label-996");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalRefreshRequestEngineKeepsResultsWithinBounds();
    }
}
