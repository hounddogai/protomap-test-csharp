namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Budget;

using Acme.Workforce.Talent.SuccessionPlan.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class SuccessionPlanPolicyEngineChecks
{
    public static void SuccessionPlanPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = SuccessionPlanPolicyEngine.DefaultSuccessionPlanPolicyEngine();
        var item = SuccessionPlanPolicy.NewSuccessionPlanPolicy("code-915", "label-375");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SuccessionPlanPolicyEngineKeepsResultsWithinBounds();
    }
}
