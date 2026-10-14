namespace Acme.Workforce.Engagement.Tests.ActionPlan.Snapshot;

using Acme.Workforce.Engagement.ActionPlan.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class StrictActionPlanThresholdEngineChecks
{
    public static void StrictActionPlanThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = StrictActionPlanThresholdEngine.DefaultStrictActionPlanThresholdEngine();
        var item = StrictActionPlanThreshold.NewStrictActionPlanThreshold("code-628", "name-645");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictActionPlanThresholdEngineKeepsResultsWithinBounds();
    }
}
