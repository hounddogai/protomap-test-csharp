namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementSummary;

using Acme.Workforce.Contractors.Engagement.EngagementSummary;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of engagement summary code, run by the project's runner.</summary>
public static class DefaultEngagementDigestEngineChecks
{
    public static void DefaultEngagementDigestEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultEngagementDigestEngine.DefaultDefaultEngagementDigestEngine(Flags.NewRegistry());
        var item = DefaultEngagementDigest.NewDefaultEngagementDigest("code-22", "label-202");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultEngagementDigestEngineKeepsResultsWithinBounds();
    }
}
