namespace Acme.Workforce.Contractors.Tests.Engagement.Digest;

using Acme.Workforce.Contractors.Engagement.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class RollingEngagementDigestEngineChecks
{
    public static void RollingEngagementDigestEngineKeepsResultsWithinBounds()
    {
        var engine = RollingEngagementDigestEngine.DefaultRollingEngagementDigestEngine();
        var item = RollingEngagementDigest.NewRollingEngagementDigest("code-581", "label-491");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingEngagementDigestEngineKeepsResultsWithinBounds();
    }
}
