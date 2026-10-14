namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackProposal;

using Acme.Workforce.Engagement.Feedback.FeedbackProposal;

/// <summary>Checks of feedback proposal code, run by the project's runner.</summary>
public static class RegionalFeedbackProposalChecks
{
    public static void RegionalFeedbackProposalStartsInPlanned()
    {
        var item = RegionalFeedbackProposal.NewRegionalFeedbackProposal("code-184", "label-33");
        Check.Equal(item.Status, RegionalFeedbackProposalStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalFeedbackProposalMovesToScheduled()
    {
        var item = RegionalFeedbackProposal.NewRegionalFeedbackProposal("code-184", "label-33");
        try
        {
            item.MoveTo(RegionalFeedbackProposalStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalFeedbackProposalStatus.Scheduled, "status");
    }

    public static void RegionalFeedbackProposalRejectsMovesFromCancelled()
    {
        var item = RegionalFeedbackProposal.NewRegionalFeedbackProposal("code-184", "label-33");
        item.Status = RegionalFeedbackProposalStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RegionalFeedbackProposalStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RegionalFeedbackProposalStartsInPlanned();
        RegionalFeedbackProposalMovesToScheduled();
        RegionalFeedbackProposalRejectsMovesFromCancelled();
    }
}
