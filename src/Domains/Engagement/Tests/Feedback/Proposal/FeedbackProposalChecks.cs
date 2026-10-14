namespace Acme.Workforce.Engagement.Tests.Feedback.Proposal;

using Acme.Workforce.Engagement.Feedback.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class FeedbackProposalChecks
{
    public static void FeedbackProposalStartsInOpen()
    {
        var item = FeedbackProposal.NewFeedbackProposal("code-123", "name-943");
        Check.Equal(item.Status, FeedbackProposalStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackProposalMovesToArchived()
    {
        var item = FeedbackProposal.NewFeedbackProposal("code-123", "name-943");
        try
        {
            item.MoveTo(FeedbackProposalStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackProposalStatus.Archived, "status");
    }

    public static void FeedbackProposalRejectsMovesFromArchived()
    {
        var item = FeedbackProposal.NewFeedbackProposal("code-123", "name-943");
        item.Status = FeedbackProposalStatus.Archived;
        Check.Fails(() => item.MoveTo(FeedbackProposalStatus.Open), "move");
    }

    public static void RunAll()
    {
        FeedbackProposalStartsInOpen();
        FeedbackProposalMovesToArchived();
        FeedbackProposalRejectsMovesFromArchived();
    }
}
