namespace Acme.Workforce.Payrollops.Tests.Payslip.Budget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Payslip.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class DefaultPayslipBudgetEngineChecks
{
    public static void DefaultPayslipBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultPayslipBudgetEngine.DefaultDefaultPayslipBudgetEngine(Flags.NewRegistry());
        var item = DefaultPayslipBudget.NewDefaultPayslipBudget("code-673", "title-470");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultPayslipBudgetEngineKeepsResultsWithinBounds();
    }
}
