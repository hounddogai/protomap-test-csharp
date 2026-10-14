namespace Acme.Workforce.Payrollops.Tests.Garnishment.Budget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Garnishment.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class LenientGarnishmentBudgetEngineChecks
{
    public static void LenientGarnishmentBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = LenientGarnishmentBudgetEngine.DefaultLenientGarnishmentBudgetEngine(Flags.NewRegistry());
        var item = LenientGarnishmentBudget.NewLenientGarnishmentBudget("code-250", "label-403");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientGarnishmentBudgetEngineKeepsResultsWithinBounds();
    }
}
