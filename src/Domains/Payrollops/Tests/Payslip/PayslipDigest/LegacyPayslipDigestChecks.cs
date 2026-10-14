namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipDigest;

using Acme.Workforce.Payrollops.Payslip.PayslipDigest;

/// <summary>Checks of payslip digest code, run by the project's runner.</summary>
public static class LegacyPayslipDigestChecks
{
    public static void LegacyPayslipDigestStartsInProposed()
    {
        var item = LegacyPayslipDigest.NewLegacyPayslipDigest("code-876", "title-810");
        Check.Equal(item.Status, LegacyPayslipDigestStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyPayslipDigestMovesToApproved()
    {
        var item = LegacyPayslipDigest.NewLegacyPayslipDigest("code-876", "title-810");
        try
        {
            item.MoveTo(LegacyPayslipDigestStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyPayslipDigestStatus.Approved, "status");
    }

    public static void LegacyPayslipDigestRejectsMovesFromWithdrawn()
    {
        var item = LegacyPayslipDigest.NewLegacyPayslipDigest("code-876", "title-810");
        item.Status = LegacyPayslipDigestStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LegacyPayslipDigestStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LegacyPayslipDigestStartsInProposed();
        LegacyPayslipDigestMovesToApproved();
        LegacyPayslipDigestRejectsMovesFromWithdrawn();
    }
}
