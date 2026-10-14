namespace Acme.Workforce.Contractors.Tests.Engagement.Score;

using Acme.Workforce.Contractors.Engagement.Score;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class EngagementDigestEngineChecks
{
    public static void EngagementDigestEngineKeepsResultsWithinBounds()
    {
        var engine = EngagementDigestEngine.DefaultEngagementDigestEngine(Flags.NewRegistry());
        var item = EngagementDigest.NewEngagementDigest("code-639", "name-312");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EngagementDigestEngineKeepsResultsWithinBounds();
    }
}
