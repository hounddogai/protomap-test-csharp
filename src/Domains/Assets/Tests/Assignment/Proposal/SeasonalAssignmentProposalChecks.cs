namespace Acme.Workforce.Assets.Tests.Assignment.Proposal;

using Acme.Workforce.Assets.Assignment.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class SeasonalAssignmentProposalChecks
{
    public static void SeasonalAssignmentProposalStartsInPlanned()
    {
        var item = SeasonalAssignmentProposal.NewSeasonalAssignmentProposal("code-808", "name-108");
        Check.Equal(item.Status, SeasonalAssignmentProposalStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalAssignmentProposalMovesToInProgress()
    {
        var item = SeasonalAssignmentProposal.NewSeasonalAssignmentProposal("code-808", "name-108");
        try
        {
            item.MoveTo(SeasonalAssignmentProposalStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalAssignmentProposalStatus.InProgress, "status");
    }

    public static void SeasonalAssignmentProposalRejectsMovesFromCancelled()
    {
        var item = SeasonalAssignmentProposal.NewSeasonalAssignmentProposal("code-808", "name-108");
        item.Status = SeasonalAssignmentProposalStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SeasonalAssignmentProposalStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SeasonalAssignmentProposalStartsInPlanned();
        SeasonalAssignmentProposalMovesToInProgress();
        SeasonalAssignmentProposalRejectsMovesFromCancelled();
    }
}
