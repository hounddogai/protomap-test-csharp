namespace Acme.Workforce.Contractors.Tests.Engagement.Profile;

using Acme.Workforce.Contractors.Engagement.Profile;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class EngagementMetricEngineChecks
{
    public static void EngagementMetricEngineKeepsResultsWithinBounds()
    {
        var engine = EngagementMetricEngine.DefaultEngagementMetricEngine(Flags.NewRegistry());
        var item = EngagementMetric.NewEngagementMetric("code-858", "name-137");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EngagementMetricEngineKeepsResultsWithinBounds();
    }
}
