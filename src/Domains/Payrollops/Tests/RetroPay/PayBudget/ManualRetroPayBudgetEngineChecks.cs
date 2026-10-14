namespace Acme.Workforce.Payrollops.Tests.RetroPay.PayBudget;

using Acme.Workforce.Payrollops.RetroPay.PayBudget;

/// <summary>Checks of pay budget code, run by the project's runner.</summary>
public static class ManualRetroPayBudgetEngineChecks
{
    public static void ManualRetroPayBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = ManualRetroPayBudgetEngine.DefaultManualRetroPayBudgetEngine();
        var item = ManualRetroPayBudget.NewManualRetroPayBudget("code-408", "label-66");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualRetroPayBudgetEngineKeepsResultsWithinBounds();
    }
}
