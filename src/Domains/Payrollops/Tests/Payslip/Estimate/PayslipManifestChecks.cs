namespace Acme.Workforce.Payrollops.Tests.Payslip.Estimate;

using Acme.Workforce.Payrollops.Payslip.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PayslipManifestChecks
{
    public static void PayslipManifestStartsInRequested()
    {
        var item = PayslipManifest.NewPayslipManifest("code-746", "title-628");
        Check.Equal(item.Status, PayslipManifestStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayslipManifestMovesToRevoked()
    {
        var item = PayslipManifest.NewPayslipManifest("code-746", "title-628");
        try
        {
            item.MoveTo(PayslipManifestStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayslipManifestStatus.Revoked, "status");
    }

    public static void PayslipManifestRejectsMovesFromExpired()
    {
        var item = PayslipManifest.NewPayslipManifest("code-746", "title-628");
        item.Status = PayslipManifestStatus.Expired;
        Check.Fails(() => item.MoveTo(PayslipManifestStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PayslipManifestStartsInRequested();
        PayslipManifestMovesToRevoked();
        PayslipManifestRejectsMovesFromExpired();
    }
}
