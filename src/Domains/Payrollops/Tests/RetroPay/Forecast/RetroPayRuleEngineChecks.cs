namespace Acme.Workforce.Payrollops.Tests.RetroPay.Forecast;

using Acme.Workforce.Payrollops.RetroPay.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class RetroPayRuleEngineChecks
{
    public static void RetroPayRuleEngineKeepsResultsWithinBounds()
    {
        var engine = RetroPayRuleEngine.DefaultRetroPayRuleEngine();
        var item = RetroPayRule.NewRetroPayRule("code-574", "title-262");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RetroPayRuleEngineKeepsResultsWithinBounds();
    }
}
