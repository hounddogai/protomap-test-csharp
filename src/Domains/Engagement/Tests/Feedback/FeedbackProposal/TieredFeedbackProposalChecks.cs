namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackProposal;

using Acme.Workforce.Engagement.Feedback.FeedbackProposal;

/// <summary>Checks of feedback proposal code, run by the project's runner.</summary>
public static class TieredFeedbackProposalChecks
{
    public static void TieredFeedbackProposalStartsInIdle()
    {
        var item = TieredFeedbackProposal.NewTieredFeedbackProposal("code-15", "name-445");
        Check.Equal(item.Status, TieredFeedbackProposalStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredFeedbackProposalMovesToStopped()
    {
        var item = TieredFeedbackProposal.NewTieredFeedbackProposal("code-15", "name-445");
        try
        {
            item.MoveTo(TieredFeedbackProposalStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredFeedbackProposalStatus.Stopped, "status");
    }

    public static void TieredFeedbackProposalRejectsMovesFromStopped()
    {
        var item = TieredFeedbackProposal.NewTieredFeedbackProposal("code-15", "name-445");
        item.Status = TieredFeedbackProposalStatus.Stopped;
        Check.Fails(() => item.MoveTo(TieredFeedbackProposalStatus.Idle), "move");
    }

    public static void RunAll()
    {
        TieredFeedbackProposalStartsInIdle();
        TieredFeedbackProposalMovesToStopped();
        TieredFeedbackProposalRejectsMovesFromStopped();
    }
}
