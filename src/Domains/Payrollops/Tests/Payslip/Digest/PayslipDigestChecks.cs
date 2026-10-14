namespace Acme.Workforce.Payrollops.Tests.Payslip.Digest;

using Acme.Workforce.Payrollops.Payslip.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class PayslipDigestChecks
{
    public static void PayslipDigestStartsInRequested()
    {
        var item = PayslipDigest.NewPayslipDigest("code-26", "label-20");
        Check.Equal(item.Status, PayslipDigestStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayslipDigestMovesToRevoked()
    {
        var item = PayslipDigest.NewPayslipDigest("code-26", "label-20");
        try
        {
            item.MoveTo(PayslipDigestStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayslipDigestStatus.Revoked, "status");
    }

    public static void PayslipDigestRejectsMovesFromExpired()
    {
        var item = PayslipDigest.NewPayslipDigest("code-26", "label-20");
        item.Status = PayslipDigestStatus.Expired;
        Check.Fails(() => item.MoveTo(PayslipDigestStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PayslipDigestStartsInRequested();
        PayslipDigestMovesToRevoked();
        PayslipDigestRejectsMovesFromExpired();
    }
}
