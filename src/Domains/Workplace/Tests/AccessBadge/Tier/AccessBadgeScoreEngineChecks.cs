namespace Acme.Workforce.Workplace.Tests.AccessBadge.Tier;

using Acme.Workforce.Workplace.AccessBadge.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class AccessBadgeScoreEngineChecks
{
    public static void AccessBadgeScoreEngineKeepsResultsWithinBounds()
    {
        var engine = AccessBadgeScoreEngine.DefaultAccessBadgeScoreEngine();
        var item = AccessBadgeScore.NewAccessBadgeScore("code-922", "name-968");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccessBadgeScoreEngineKeepsResultsWithinBounds();
    }
}
