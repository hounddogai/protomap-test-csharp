namespace Acme.Workforce.Engagement.Tests.ActionPlan.Preference;

using Acme.Workforce.Engagement.ActionPlan.Preference;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ActionPlanPreferenceEngineChecks
{
    public static void ActionPlanPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = ActionPlanPreferenceEngine.DefaultActionPlanPreferenceEngine(Flags.NewRegistry());
        var item = ActionPlanPreference.NewActionPlanPreference("code-296", "name-396");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ActionPlanPreferenceEngineKeepsResultsWithinBounds();
    }
}
