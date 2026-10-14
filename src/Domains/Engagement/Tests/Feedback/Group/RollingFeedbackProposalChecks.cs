namespace Acme.Workforce.Engagement.Tests.Feedback.Group;

using Acme.Workforce.Engagement.Feedback.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class RollingFeedbackProposalChecks
{
    public static void RollingFeedbackProposalStartsInCreated()
    {
        var item = RollingFeedbackProposal.NewRollingFeedbackProposal("code-772", "label-193");
        Check.Equal(item.Status, RollingFeedbackProposalStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingFeedbackProposalMovesToPublished()
    {
        var item = RollingFeedbackProposal.NewRollingFeedbackProposal("code-772", "label-193");
        try
        {
            item.MoveTo(RollingFeedbackProposalStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingFeedbackProposalStatus.Published, "status");
    }

    public static void RollingFeedbackProposalRejectsMovesFromRetired()
    {
        var item = RollingFeedbackProposal.NewRollingFeedbackProposal("code-772", "label-193");
        item.Status = RollingFeedbackProposalStatus.Retired;
        Check.Fails(() => item.MoveTo(RollingFeedbackProposalStatus.Created), "move");
    }

    public static void RunAll()
    {
        RollingFeedbackProposalStartsInCreated();
        RollingFeedbackProposalMovesToPublished();
        RollingFeedbackProposalRejectsMovesFromRetired();
    }
}
