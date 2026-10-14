namespace Acme.Workforce.Assets.Tests.Refresh.Request;

using Acme.Workforce.Assets.Refresh.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class PendingRefreshMappingEngineChecks
{
    public static void PendingRefreshMappingEngineKeepsResultsWithinBounds()
    {
        var engine = PendingRefreshMappingEngine.DefaultPendingRefreshMappingEngine();
        var item = PendingRefreshMapping.NewPendingRefreshMapping("code-902", "label-311");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingRefreshMappingEngineKeepsResultsWithinBounds();
    }
}
