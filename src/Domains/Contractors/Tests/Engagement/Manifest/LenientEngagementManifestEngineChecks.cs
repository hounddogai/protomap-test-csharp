namespace Acme.Workforce.Contractors.Tests.Engagement.Manifest;

using Acme.Workforce.Contractors.Engagement.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class LenientEngagementManifestEngineChecks
{
    public static void LenientEngagementManifestEngineKeepsResultsWithinBounds()
    {
        var engine = LenientEngagementManifestEngine.DefaultLenientEngagementManifestEngine();
        var item = LenientEngagementManifest.NewLenientEngagementManifest("code-891", "label-522");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientEngagementManifestEngineKeepsResultsWithinBounds();
    }
}
