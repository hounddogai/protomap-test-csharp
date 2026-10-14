namespace Acme.Workforce.Payrollops.Tests.Payslip.Checkpoint;

using Acme.Workforce.Payrollops.Payslip.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class PayslipProposalChecks
{
    public static void PayslipProposalStartsInRequested()
    {
        var item = PayslipProposal.NewPayslipProposal("code-18", "title-345");
        Check.Equal(item.Status, PayslipProposalStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayslipProposalMovesToExpired()
    {
        var item = PayslipProposal.NewPayslipProposal("code-18", "title-345");
        try
        {
            item.MoveTo(PayslipProposalStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayslipProposalStatus.Expired, "status");
    }

    public static void PayslipProposalRejectsMovesFromExpired()
    {
        var item = PayslipProposal.NewPayslipProposal("code-18", "title-345");
        item.Status = PayslipProposalStatus.Expired;
        Check.Fails(() => item.MoveTo(PayslipProposalStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PayslipProposalStartsInRequested();
        PayslipProposalMovesToExpired();
        PayslipProposalRejectsMovesFromExpired();
    }
}
