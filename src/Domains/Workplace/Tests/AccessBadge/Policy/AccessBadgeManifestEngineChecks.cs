namespace Acme.Workforce.Workplace.Tests.AccessBadge.Policy;

using Acme.Workforce.Workplace.AccessBadge.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class AccessBadgeManifestEngineChecks
{
    public static void AccessBadgeManifestEngineKeepsResultsWithinBounds()
    {
        var engine = AccessBadgeManifestEngine.DefaultAccessBadgeManifestEngine();
        var item = AccessBadgeManifest.NewAccessBadgeManifest("code-997", "label-157");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccessBadgeManifestEngineKeepsResultsWithinBounds();
    }
}
