namespace Acme.Workforce.Engagement.Tests.ActionPlan.Policy;

using Acme.Workforce.Engagement.ActionPlan.Policy;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class DefaultActionPlanPolicyEngineChecks
{
    public static void DefaultActionPlanPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultActionPlanPolicyEngine.DefaultDefaultActionPlanPolicyEngine(Flags.NewRegistry());
        var item = DefaultActionPlanPolicy.NewDefaultActionPlanPolicy("code-938", "label-25");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultActionPlanPolicyEngineKeepsResultsWithinBounds();
    }
}
