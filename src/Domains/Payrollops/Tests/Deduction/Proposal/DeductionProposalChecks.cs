namespace Acme.Workforce.Payrollops.Tests.Deduction.Proposal;

using Acme.Workforce.Payrollops.Deduction.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DeductionProposalChecks
{
    public static void DeductionProposalStartsInProposed()
    {
        var item = DeductionProposal.NewDeductionProposal("code-842", "name-322");
        Check.Equal(item.Status, DeductionProposalStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionProposalMovesToRejected()
    {
        var item = DeductionProposal.NewDeductionProposal("code-842", "name-322");
        try
        {
            item.MoveTo(DeductionProposalStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionProposalStatus.Rejected, "status");
    }

    public static void DeductionProposalRejectsMovesFromWithdrawn()
    {
        var item = DeductionProposal.NewDeductionProposal("code-842", "name-322");
        item.Status = DeductionProposalStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(DeductionProposalStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        DeductionProposalStartsInProposed();
        DeductionProposalMovesToRejected();
        DeductionProposalRejectsMovesFromWithdrawn();
    }
}
