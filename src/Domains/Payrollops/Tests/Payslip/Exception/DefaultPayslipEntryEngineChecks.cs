namespace Acme.Workforce.Payrollops.Tests.Payslip.Exception;

using Acme.Workforce.Payrollops.Payslip.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class DefaultPayslipEntryEngineChecks
{
    public static void DefaultPayslipEntryEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultPayslipEntryEngine.DefaultDefaultPayslipEntryEngine();
        var item = DefaultPayslipEntry.NewDefaultPayslipEntry("code-971", "name-55");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultPayslipEntryEngineKeepsResultsWithinBounds();
    }
}
