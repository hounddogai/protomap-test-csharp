namespace Acme.Workforce.Assets.Tests.Refresh.Tier;

using Acme.Workforce.Assets.Refresh.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class RefreshTierEngineChecks
{
    public static void RefreshTierEngineKeepsResultsWithinBounds()
    {
        var engine = RefreshTierEngine.DefaultRefreshTierEngine();
        var item = RefreshTier.NewRefreshTier("code-486", "title-486");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RefreshTierEngineKeepsResultsWithinBounds();
    }
}
