namespace Acme.Workforce.Payrollops.Tests.RetroPay.Budget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.RetroPay.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class RetroPayBudgetEngineChecks
{
    public static void RetroPayBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = RetroPayBudgetEngine.DefaultRetroPayBudgetEngine(Flags.NewRegistry());
        var item = RetroPayBudget.NewRetroPayBudget("code-911", "label-202");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RetroPayBudgetEngineKeepsResultsWithinBounds();
    }
}
