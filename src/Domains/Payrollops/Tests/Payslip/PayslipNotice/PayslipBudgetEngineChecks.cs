namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipNotice;

using Acme.Workforce.Payrollops.Payslip.PayslipNotice;

/// <summary>Checks of payslip notice code, run by the project's runner.</summary>
public static class PayslipBudgetEngineChecks
{
    public static void PayslipBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = PayslipBudgetEngine.DefaultPayslipBudgetEngine();
        var item = PayslipBudget.NewPayslipBudget("code-872", "title-911");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayslipBudgetEngineKeepsResultsWithinBounds();
    }
}
