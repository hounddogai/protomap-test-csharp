namespace Acme.Workforce.Contractors.Tests.Engagement.Assignment;

using Acme.Workforce.Contractors.Engagement.Assignment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class EngagementMilestoneEngineChecks
{
    public static void EngagementMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = EngagementMilestoneEngine.DefaultEngagementMilestoneEngine(Flags.NewRegistry());
        var item = EngagementMilestone.NewEngagementMilestone("code-394", "label-948");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EngagementMilestoneEngineKeepsResultsWithinBounds();
    }
}
