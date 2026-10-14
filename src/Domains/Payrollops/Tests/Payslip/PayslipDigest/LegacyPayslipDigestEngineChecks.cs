namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipDigest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Payslip.PayslipDigest;

/// <summary>Checks of payslip digest code, run by the project's runner.</summary>
public static class LegacyPayslipDigestEngineChecks
{
    public static void LegacyPayslipDigestEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyPayslipDigestEngine.DefaultLegacyPayslipDigestEngine(Flags.NewRegistry());
        var item = LegacyPayslipDigest.NewLegacyPayslipDigest("code-876", "title-810");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyPayslipDigestEngineKeepsResultsWithinBounds();
    }
}
