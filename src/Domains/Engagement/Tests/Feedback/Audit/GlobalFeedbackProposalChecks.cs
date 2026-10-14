namespace Acme.Workforce.Engagement.Tests.Feedback.Audit;

using Acme.Workforce.Engagement.Feedback.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class GlobalFeedbackProposalChecks
{
    public static void GlobalFeedbackProposalStartsInOpen()
    {
        var item = GlobalFeedbackProposal.NewGlobalFeedbackProposal("code-123", "name-198");
        Check.Equal(item.Status, GlobalFeedbackProposalStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalFeedbackProposalMovesToReopened()
    {
        var item = GlobalFeedbackProposal.NewGlobalFeedbackProposal("code-123", "name-198");
        try
        {
            item.MoveTo(GlobalFeedbackProposalStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalFeedbackProposalStatus.Reopened, "status");
    }

    public static void GlobalFeedbackProposalRejectsMovesFromArchived()
    {
        var item = GlobalFeedbackProposal.NewGlobalFeedbackProposal("code-123", "name-198");
        item.Status = GlobalFeedbackProposalStatus.Archived;
        Check.Fails(() => item.MoveTo(GlobalFeedbackProposalStatus.Open), "move");
    }

    public static void RunAll()
    {
        GlobalFeedbackProposalStartsInOpen();
        GlobalFeedbackProposalMovesToReopened();
        GlobalFeedbackProposalRejectsMovesFromArchived();
    }
}
