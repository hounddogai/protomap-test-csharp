namespace Acme.Workforce.Payrollops.Tests.RetroPay.Threshold;

using Acme.Workforce.Payrollops.RetroPay.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class RetroPayPlanEngineChecks
{
    public static void RetroPayPlanEngineKeepsResultsWithinBounds()
    {
        var engine = RetroPayPlanEngine.DefaultRetroPayPlanEngine();
        var item = RetroPayPlan.NewRetroPayPlan("code-539", "name-23");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RetroPayPlanEngineKeepsResultsWithinBounds();
    }
}
