namespace Acme.Workforce.Assets.Tests.Refresh.Preference;

using Acme.Workforce.Assets.Refresh.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RefreshDigestEngineChecks
{
    public static void RefreshDigestEngineKeepsResultsWithinBounds()
    {
        var engine = RefreshDigestEngine.DefaultRefreshDigestEngine();
        var item = RefreshDigest.NewRefreshDigest("code-709", "title-906");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RefreshDigestEngineKeepsResultsWithinBounds();
    }
}
