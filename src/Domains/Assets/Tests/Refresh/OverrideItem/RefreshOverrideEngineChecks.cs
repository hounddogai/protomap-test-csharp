namespace Acme.Workforce.Assets.Tests.Refresh.OverrideItem;

using Acme.Workforce.Assets.Refresh.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RefreshOverrideEngineChecks
{
    public static void RefreshOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = RefreshOverrideEngine.DefaultRefreshOverrideEngine();
        var item = RefreshOverride.NewRefreshOverride("code-616", "label-528");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RefreshOverrideEngineKeepsResultsWithinBounds();
    }
}
