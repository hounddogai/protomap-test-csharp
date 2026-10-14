namespace Acme.Workforce.Contractors.Tests.Engagement.Summary;

using Acme.Workforce.Contractors.Engagement.Summary;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class CappedEngagementSummaryEngineChecks
{
    public static void CappedEngagementSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = CappedEngagementSummaryEngine.DefaultCappedEngagementSummaryEngine(Flags.NewRegistry());
        var item = CappedEngagementSummary.NewCappedEngagementSummary("code-89", "label-840");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedEngagementSummaryEngineKeepsResultsWithinBounds();
    }
}
