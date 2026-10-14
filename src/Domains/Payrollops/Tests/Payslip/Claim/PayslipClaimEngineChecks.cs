namespace Acme.Workforce.Payrollops.Tests.Payslip.Claim;

using Acme.Workforce.Payrollops.Payslip.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class PayslipClaimEngineChecks
{
    public static void PayslipClaimEngineKeepsResultsWithinBounds()
    {
        var engine = PayslipClaimEngine.DefaultPayslipClaimEngine();
        var item = PayslipClaim.NewPayslipClaim("code-460", "label-113");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayslipClaimEngineKeepsResultsWithinBounds();
    }
}
