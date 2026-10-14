namespace Acme.Workforce.Payrollops.Tests.Payslip.Checkpoint;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Payslip.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class CappedPayslipBatchEngineChecks
{
    public static void CappedPayslipBatchEngineKeepsResultsWithinBounds()
    {
        var engine = CappedPayslipBatchEngine.DefaultCappedPayslipBatchEngine(Flags.NewRegistry());
        var item = CappedPayslipBatch.NewCappedPayslipBatch("code-23", "label-81");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedPayslipBatchEngineKeepsResultsWithinBounds();
    }
}
