namespace Acme.Workforce.Engagement.Tests.ActionPlan.Batch;

using Acme.Workforce.Engagement.ActionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class DefaultActionPlanManifestEngineChecks
{
    public static void DefaultActionPlanManifestEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultActionPlanManifestEngine.DefaultDefaultActionPlanManifestEngine();
        var item = DefaultActionPlanManifest.NewDefaultActionPlanManifest("code-988", "name-485");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultActionPlanManifestEngineKeepsResultsWithinBounds();
    }
}
