namespace Acme.Workforce.Payrollops.Tests.Payslip.Digest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Payslip.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class PayslipDigestEngineChecks
{
    public static void PayslipDigestEngineKeepsResultsWithinBounds()
    {
        var engine = PayslipDigestEngine.DefaultPayslipDigestEngine(Flags.NewRegistry());
        var item = PayslipDigest.NewPayslipDigest("code-26", "label-20");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayslipDigestEngineKeepsResultsWithinBounds();
    }
}
