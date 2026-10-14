namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipAssignment;

using Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

/// <summary>Checks of payslip assignment code, run by the project's runner.</summary>
public static class FallbackPayslipDigestChecks
{
    public static void FallbackPayslipDigestStartsInCreated()
    {
        var item = FallbackPayslipDigest.NewFallbackPayslipDigest("code-804", "name-775");
        Check.Equal(item.Status, FallbackPayslipDigestStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackPayslipDigestMovesToValidated()
    {
        var item = FallbackPayslipDigest.NewFallbackPayslipDigest("code-804", "name-775");
        try
        {
            item.MoveTo(FallbackPayslipDigestStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackPayslipDigestStatus.Validated, "status");
    }

    public static void FallbackPayslipDigestRejectsMovesFromRetired()
    {
        var item = FallbackPayslipDigest.NewFallbackPayslipDigest("code-804", "name-775");
        item.Status = FallbackPayslipDigestStatus.Retired;
        Check.Fails(() => item.MoveTo(FallbackPayslipDigestStatus.Created), "move");
    }

    public static void RunAll()
    {
        FallbackPayslipDigestStartsInCreated();
        FallbackPayslipDigestMovesToValidated();
        FallbackPayslipDigestRejectsMovesFromRetired();
    }
}
