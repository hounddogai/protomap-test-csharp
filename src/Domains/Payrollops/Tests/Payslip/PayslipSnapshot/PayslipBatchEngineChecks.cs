namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipSnapshot;

using Acme.Workforce.Payrollops.Payslip.PayslipSnapshot;

/// <summary>Checks of payslip snapshot code, run by the project's runner.</summary>
public static class PayslipBatchEngineChecks
{
    public static void PayslipBatchEngineKeepsResultsWithinBounds()
    {
        var engine = PayslipBatchEngine.DefaultPayslipBatchEngine();
        var item = PayslipBatch.NewPayslipBatch("code-142", "title-759");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayslipBatchEngineKeepsResultsWithinBounds();
    }
}
