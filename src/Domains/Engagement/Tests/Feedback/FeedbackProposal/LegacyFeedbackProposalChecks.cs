namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackProposal;

using Acme.Workforce.Engagement.Feedback.FeedbackProposal;

/// <summary>Checks of feedback proposal code, run by the project's runner.</summary>
public static class LegacyFeedbackProposalChecks
{
    public static void LegacyFeedbackProposalStartsInProposed()
    {
        var item = LegacyFeedbackProposal.NewLegacyFeedbackProposal("code-195", "label-553");
        Check.Equal(item.Status, LegacyFeedbackProposalStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyFeedbackProposalMovesToWithdrawn()
    {
        var item = LegacyFeedbackProposal.NewLegacyFeedbackProposal("code-195", "label-553");
        try
        {
            item.MoveTo(LegacyFeedbackProposalStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyFeedbackProposalStatus.Withdrawn, "status");
    }

    public static void LegacyFeedbackProposalRejectsMovesFromWithdrawn()
    {
        var item = LegacyFeedbackProposal.NewLegacyFeedbackProposal("code-195", "label-553");
        item.Status = LegacyFeedbackProposalStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LegacyFeedbackProposalStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LegacyFeedbackProposalStartsInProposed();
        LegacyFeedbackProposalMovesToWithdrawn();
        LegacyFeedbackProposalRejectsMovesFromWithdrawn();
    }
}
