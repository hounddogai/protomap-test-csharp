namespace Acme.Workforce.Payrollops.Tests.Payslip.Batch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Payslip.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class FallbackPayslipBatchEngineChecks
{
    public static void FallbackPayslipBatchEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackPayslipBatchEngine.DefaultFallbackPayslipBatchEngine(Flags.NewRegistry());
        var item = FallbackPayslipBatch.NewFallbackPayslipBatch("code-371", "name-979");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackPayslipBatchEngineKeepsResultsWithinBounds();
    }
}
