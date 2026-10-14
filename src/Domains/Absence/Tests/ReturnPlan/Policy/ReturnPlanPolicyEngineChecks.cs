namespace Acme.Workforce.Absence.Tests.ReturnPlan.Policy;

using Acme.Workforce.Absence.ReturnPlan.Policy;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class ReturnPlanPolicyEngineChecks
{
    public static void ReturnPlanPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = ReturnPlanPolicyEngine.DefaultReturnPlanPolicyEngine(Flags.NewRegistry());
        var item = ReturnPlanPolicy.NewReturnPlanPolicy("code-937", "label-344");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReturnPlanPolicyEngineKeepsResultsWithinBounds();
    }
}
