namespace Acme.Workforce.Workplace.Tests.AccessBadge.Tier;

using Acme.Workforce.Workplace.AccessBadge.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class DefaultAccessBadgeManifestEngineChecks
{
    public static void DefaultAccessBadgeManifestEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultAccessBadgeManifestEngine.DefaultDefaultAccessBadgeManifestEngine();
        var item = DefaultAccessBadgeManifest.NewDefaultAccessBadgeManifest("code-720", "label-694");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultAccessBadgeManifestEngineKeepsResultsWithinBounds();
    }
}
