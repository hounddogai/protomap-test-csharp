namespace Acme.Workforce.Engagement.Tests.ActionPlan.Preference;

using Acme.Workforce.Engagement.ActionPlan.Preference;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ActionPlanBaselineEngineChecks
{
    public static void ActionPlanBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = ActionPlanBaselineEngine.DefaultActionPlanBaselineEngine(Flags.NewRegistry());
        var item = ActionPlanBaseline.NewActionPlanBaseline("code-312", "label-658");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ActionPlanBaselineEngineKeepsResultsWithinBounds();
    }
}
