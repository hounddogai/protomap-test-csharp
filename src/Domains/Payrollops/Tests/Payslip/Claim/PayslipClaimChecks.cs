namespace Acme.Workforce.Payrollops.Tests.Payslip.Claim;

using Acme.Workforce.Payrollops.Payslip.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class PayslipClaimChecks
{
    public static void PayslipClaimStartsInDraft()
    {
        var item = PayslipClaim.NewPayslipClaim("code-460", "label-113");
        Check.Equal(item.Status, PayslipClaimStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayslipClaimMovesToSuspended()
    {
        var item = PayslipClaim.NewPayslipClaim("code-460", "label-113");
        try
        {
            item.MoveTo(PayslipClaimStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayslipClaimStatus.Suspended, "status");
    }

    public static void PayslipClaimRejectsMovesFromClosed()
    {
        var item = PayslipClaim.NewPayslipClaim("code-460", "label-113");
        item.Status = PayslipClaimStatus.Closed;
        Check.Fails(() => item.MoveTo(PayslipClaimStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PayslipClaimStartsInDraft();
        PayslipClaimMovesToSuspended();
        PayslipClaimRejectsMovesFromClosed();
    }
}
