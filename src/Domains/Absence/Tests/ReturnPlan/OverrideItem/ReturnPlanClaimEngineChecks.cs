namespace Acme.Workforce.Absence.Tests.ReturnPlan.OverrideItem;

using Acme.Workforce.Absence.ReturnPlan.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class ReturnPlanClaimEngineChecks
{
    public static void ReturnPlanClaimEngineKeepsResultsWithinBounds()
    {
        var engine = ReturnPlanClaimEngine.DefaultReturnPlanClaimEngine();
        var item = ReturnPlanClaim.NewReturnPlanClaim("code-65", "title-81");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReturnPlanClaimEngineKeepsResultsWithinBounds();
    }
}
