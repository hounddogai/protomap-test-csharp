namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeEntry;

using Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

/// <summary>Checks of badge entry code, run by the project's runner.</summary>
public static class RegionalAccessBadgeScoreEngineChecks
{
    public static void RegionalAccessBadgeScoreEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalAccessBadgeScoreEngine.DefaultRegionalAccessBadgeScoreEngine();
        var item = RegionalAccessBadgeScore.NewRegionalAccessBadgeScore("code-700", "name-363");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalAccessBadgeScoreEngineKeepsResultsWithinBounds();
    }
}
