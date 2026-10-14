namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementSummary;

using Acme.Workforce.Contractors.Engagement.EngagementSummary;

/// <summary>Checks of engagement summary code, run by the project's runner.</summary>
public static class PrimaryEngagementSummaryEngineChecks
{
    public static void PrimaryEngagementSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryEngagementSummaryEngine.DefaultPrimaryEngagementSummaryEngine();
        var item = PrimaryEngagementSummary.NewPrimaryEngagementSummary("code-255", "title-268");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryEngagementSummaryEngineKeepsResultsWithinBounds();
    }
}
