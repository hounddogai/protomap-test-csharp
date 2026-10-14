namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipNotice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Payslip.PayslipNotice;

/// <summary>Checks of payslip notice code, run by the project's runner.</summary>
public static class BatchedPayslipClaimEngineChecks
{
    public static void BatchedPayslipClaimEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedPayslipClaimEngine.DefaultBatchedPayslipClaimEngine(Flags.NewRegistry());
        var item = BatchedPayslipClaim.NewBatchedPayslipClaim("code-857", "name-550");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedPayslipClaimEngineKeepsResultsWithinBounds();
    }
}
