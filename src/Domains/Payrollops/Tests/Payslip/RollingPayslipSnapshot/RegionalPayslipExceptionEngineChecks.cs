namespace Acme.Workforce.Payrollops.Tests.Payslip.RollingPayslipSnapshot;

using Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

/// <summary>Checks of rolling payslip snapshot code, run by the project's runner.</summary>
public static class RegionalPayslipExceptionEngineChecks
{
    public static void RegionalPayslipExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalPayslipExceptionEngine.DefaultRegionalPayslipExceptionEngine();
        var item = RegionalPayslipException.NewRegionalPayslipException("code-463", "label-234");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalPayslipExceptionEngineKeepsResultsWithinBounds();
    }
}
