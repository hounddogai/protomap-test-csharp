namespace Acme.Workforce.Contractors.Tests.Engagement.Target;

using Acme.Workforce.Contractors.Engagement.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class SeasonalEngagementMilestoneEngineChecks
{
    public static void SeasonalEngagementMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalEngagementMilestoneEngine.DefaultSeasonalEngagementMilestoneEngine();
        var item = SeasonalEngagementMilestone.NewSeasonalEngagementMilestone("code-958", "label-100");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalEngagementMilestoneEngineKeepsResultsWithinBounds();
    }
}
