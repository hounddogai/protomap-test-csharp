namespace Acme.Workforce.Contractors.Tests.Engagement.Notice;

using Acme.Workforce.Contractors.Engagement.Notice;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class SeasonalEngagementRollupEngineChecks
{
    public static void SeasonalEngagementRollupEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalEngagementRollupEngine.DefaultSeasonalEngagementRollupEngine(Flags.NewRegistry());
        var item = SeasonalEngagementRollup.NewSeasonalEngagementRollup("code-601", "title-56");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalEngagementRollupEngineKeepsResultsWithinBounds();
    }
}
