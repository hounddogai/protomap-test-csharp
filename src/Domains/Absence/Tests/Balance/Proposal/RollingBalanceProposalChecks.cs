namespace Acme.Workforce.Absence.Tests.Balance.Proposal;

using Acme.Workforce.Absence.Balance.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RollingBalanceProposalChecks
{
    public static void RollingBalanceProposalStartsInRequested()
    {
        var item = RollingBalanceProposal.NewRollingBalanceProposal("code-741", "label-939");
        Check.Equal(item.Status, RollingBalanceProposalStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingBalanceProposalMovesToGranted()
    {
        var item = RollingBalanceProposal.NewRollingBalanceProposal("code-741", "label-939");
        try
        {
            item.MoveTo(RollingBalanceProposalStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingBalanceProposalStatus.Granted, "status");
    }

    public static void RollingBalanceProposalRejectsMovesFromExpired()
    {
        var item = RollingBalanceProposal.NewRollingBalanceProposal("code-741", "label-939");
        item.Status = RollingBalanceProposalStatus.Expired;
        Check.Fails(() => item.MoveTo(RollingBalanceProposalStatus.Requested), "move");
    }

    public static void RunAll()
    {
        RollingBalanceProposalStartsInRequested();
        RollingBalanceProposalMovesToGranted();
        RollingBalanceProposalRejectsMovesFromExpired();
    }
}
