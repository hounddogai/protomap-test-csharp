namespace Acme.Workforce.Engagement.Tests.Feedback.Mapping;

using Acme.Workforce.Engagement.Feedback.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class PendingFeedbackMilestoneChecks
{
    public static void PendingFeedbackMilestoneStartsInProposed()
    {
        var item = PendingFeedbackMilestone.NewPendingFeedbackMilestone("code-51", "name-723");
        Check.Equal(item.Status, PendingFeedbackMilestoneStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingFeedbackMilestoneMovesToWithdrawn()
    {
        var item = PendingFeedbackMilestone.NewPendingFeedbackMilestone("code-51", "name-723");
        try
        {
            item.MoveTo(PendingFeedbackMilestoneStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingFeedbackMilestoneStatus.Withdrawn, "status");
    }

    public static void PendingFeedbackMilestoneRejectsMovesFromWithdrawn()
    {
        var item = PendingFeedbackMilestone.NewPendingFeedbackMilestone("code-51", "name-723");
        item.Status = PendingFeedbackMilestoneStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PendingFeedbackMilestoneStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PendingFeedbackMilestoneStartsInProposed();
        PendingFeedbackMilestoneMovesToWithdrawn();
        PendingFeedbackMilestoneRejectsMovesFromWithdrawn();
    }
}
