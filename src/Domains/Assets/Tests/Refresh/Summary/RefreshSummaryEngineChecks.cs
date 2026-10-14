namespace Acme.Workforce.Assets.Tests.Refresh.Summary;

using Acme.Workforce.Assets.Refresh.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class RefreshSummaryEngineChecks
{
    public static void RefreshSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = RefreshSummaryEngine.DefaultRefreshSummaryEngine();
        var item = RefreshSummary.NewRefreshSummary("code-489", "label-615");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RefreshSummaryEngineKeepsResultsWithinBounds();
    }
}
