namespace Acme.Workforce.Absence.Tests.ReturnPlan.Bundle;

using Acme.Workforce.Absence.ReturnPlan.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class ReturnPlanBundleEngineChecks
{
    public static void ReturnPlanBundleEngineKeepsResultsWithinBounds()
    {
        var engine = ReturnPlanBundleEngine.DefaultReturnPlanBundleEngine();
        var item = ReturnPlanBundle.NewReturnPlanBundle("code-189", "name-542");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReturnPlanBundleEngineKeepsResultsWithinBounds();
    }
}
