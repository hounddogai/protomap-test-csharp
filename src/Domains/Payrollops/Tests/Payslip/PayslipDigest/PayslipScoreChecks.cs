namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipDigest;

using Acme.Workforce.Payrollops.Payslip.PayslipDigest;

/// <summary>Checks of payslip digest code, run by the project's runner.</summary>
public static class PayslipScoreChecks
{
    public static void PayslipScoreStartsInRequested()
    {
        var item = PayslipScore.NewPayslipScore("code-43", "label-992");
        Check.Equal(item.Status, PayslipScoreStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayslipScoreMovesToRevoked()
    {
        var item = PayslipScore.NewPayslipScore("code-43", "label-992");
        try
        {
            item.MoveTo(PayslipScoreStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayslipScoreStatus.Revoked, "status");
    }

    public static void PayslipScoreRejectsMovesFromExpired()
    {
        var item = PayslipScore.NewPayslipScore("code-43", "label-992");
        item.Status = PayslipScoreStatus.Expired;
        Check.Fails(() => item.MoveTo(PayslipScoreStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PayslipScoreStartsInRequested();
        PayslipScoreMovesToRevoked();
        PayslipScoreRejectsMovesFromExpired();
    }
}
