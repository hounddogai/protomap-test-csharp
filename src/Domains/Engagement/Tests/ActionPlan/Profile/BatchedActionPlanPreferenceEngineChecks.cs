namespace Acme.Workforce.Engagement.Tests.ActionPlan.Profile;

using Acme.Workforce.Engagement.ActionPlan.Profile;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class BatchedActionPlanPreferenceEngineChecks
{
    public static void BatchedActionPlanPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedActionPlanPreferenceEngine.DefaultBatchedActionPlanPreferenceEngine(Flags.NewRegistry());
        var item = BatchedActionPlanPreference.NewBatchedActionPlanPreference("code-745", "label-844");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedActionPlanPreferenceEngineKeepsResultsWithinBounds();
    }
}
