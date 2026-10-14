namespace Acme.Workforce.Talent.Tests.CareerPath.GlobalPathRule;

using Acme.Workforce.Talent.CareerPath.GlobalPathRule;

/// <summary>Checks of global path rule code, run by the project's runner.</summary>
public static class StrictCareerPathProposalChecks
{
    public static void StrictCareerPathProposalStartsInDraft()
    {
        var item = StrictCareerPathProposal.NewStrictCareerPathProposal("code-178", "title-278");
        Check.Equal(item.Status, StrictCareerPathProposalStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictCareerPathProposalMovesToActive()
    {
        var item = StrictCareerPathProposal.NewStrictCareerPathProposal("code-178", "title-278");
        try
        {
            item.MoveTo(StrictCareerPathProposalStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictCareerPathProposalStatus.Active, "status");
    }

    public static void StrictCareerPathProposalRejectsMovesFromClosed()
    {
        var item = StrictCareerPathProposal.NewStrictCareerPathProposal("code-178", "title-278");
        item.Status = StrictCareerPathProposalStatus.Closed;
        Check.Fails(() => item.MoveTo(StrictCareerPathProposalStatus.Draft), "move");
    }

    public static void RunAll()
    {
        StrictCareerPathProposalStartsInDraft();
        StrictCareerPathProposalMovesToActive();
        StrictCareerPathProposalRejectsMovesFromClosed();
    }
}
