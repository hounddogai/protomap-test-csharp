namespace Acme.Workforce.Assets.Tests.Refresh.Request;

using Acme.Workforce.Assets.Refresh.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class RollingRefreshPreferenceEngineChecks
{
    public static void RollingRefreshPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = RollingRefreshPreferenceEngine.DefaultRollingRefreshPreferenceEngine();
        var item = RollingRefreshPreference.NewRollingRefreshPreference("code-129", "label-427");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingRefreshPreferenceEngineKeepsResultsWithinBounds();
    }
}
