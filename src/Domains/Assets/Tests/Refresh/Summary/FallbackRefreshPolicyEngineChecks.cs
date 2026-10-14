namespace Acme.Workforce.Assets.Tests.Refresh.Summary;

using Acme.Workforce.Assets.Refresh.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class FallbackRefreshPolicyEngineChecks
{
    public static void FallbackRefreshPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackRefreshPolicyEngine.DefaultFallbackRefreshPolicyEngine();
        var item = FallbackRefreshPolicy.NewFallbackRefreshPolicy("code-172", "label-690");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackRefreshPolicyEngineKeepsResultsWithinBounds();
    }
}
