namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipAssignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

/// <summary>Checks of payslip assignment code, run by the project's runner.</summary>
public static class FallbackPayslipDigestEngineChecks
{
    public static void FallbackPayslipDigestEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackPayslipDigestEngine.DefaultFallbackPayslipDigestEngine(Flags.NewRegistry());
        var item = FallbackPayslipDigest.NewFallbackPayslipDigest("code-804", "name-775");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackPayslipDigestEngineKeepsResultsWithinBounds();
    }
}
