namespace Acme.Workforce.Orgdesign.Tests.Headcount.Proposal;

using Acme.Workforce.Orgdesign.Headcount.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class HeadcountProposalChecks
{
    public static void HeadcountProposalStartsInRequested()
    {
        var item = HeadcountProposal.NewHeadcountProposal("code-146", "title-693");
        Check.Equal(item.Status, HeadcountProposalStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountProposalMovesToRevoked()
    {
        var item = HeadcountProposal.NewHeadcountProposal("code-146", "title-693");
        try
        {
            item.MoveTo(HeadcountProposalStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountProposalStatus.Revoked, "status");
    }

    public static void HeadcountProposalRejectsMovesFromExpired()
    {
        var item = HeadcountProposal.NewHeadcountProposal("code-146", "title-693");
        item.Status = HeadcountProposalStatus.Expired;
        Check.Fails(() => item.MoveTo(HeadcountProposalStatus.Requested), "move");
    }

    public static void RunAll()
    {
        HeadcountProposalStartsInRequested();
        HeadcountProposalMovesToRevoked();
        HeadcountProposalRejectsMovesFromExpired();
    }
}
