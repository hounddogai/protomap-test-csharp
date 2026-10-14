namespace Acme.Workforce.Payrollops.Tests.Payslip.Slot;

using Acme.Workforce.Payrollops.Payslip.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class PayslipRequestChecks
{
    public static void PayslipRequestStartsInProposed()
    {
        var item = PayslipRequest.NewPayslipRequest("code-917", "title-894");
        Check.Equal(item.Status, PayslipRequestStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayslipRequestMovesToApproved()
    {
        var item = PayslipRequest.NewPayslipRequest("code-917", "title-894");
        try
        {
            item.MoveTo(PayslipRequestStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayslipRequestStatus.Approved, "status");
    }

    public static void PayslipRequestRejectsMovesFromWithdrawn()
    {
        var item = PayslipRequest.NewPayslipRequest("code-917", "title-894");
        item.Status = PayslipRequestStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PayslipRequestStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PayslipRequestStartsInProposed();
        PayslipRequestMovesToApproved();
        PayslipRequestRejectsMovesFromWithdrawn();
    }
}
