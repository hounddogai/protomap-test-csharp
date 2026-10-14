namespace Acme.Workforce.Engagement.Tests.ActionPlan.Profile;

using Acme.Workforce.Engagement.ActionPlan.Profile;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PendingActionPlanBaselineEngineChecks
{
    public static void PendingActionPlanBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = PendingActionPlanBaselineEngine.DefaultPendingActionPlanBaselineEngine(Flags.NewRegistry());
        var item = PendingActionPlanBaseline.NewPendingActionPlanBaseline("code-385", "name-465");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingActionPlanBaselineEngineKeepsResultsWithinBounds();
    }
}
