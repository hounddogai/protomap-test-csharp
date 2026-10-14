namespace Acme.Workforce.Engagement.Tests.Feedback.Reservation;

using Acme.Workforce.Engagement.Feedback.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class ScheduledFeedbackRecordChecks
{
    public static void ScheduledFeedbackRecordStartsInProposed()
    {
        var item = ScheduledFeedbackRecord.NewScheduledFeedbackRecord("code-702", "label-142");
        Check.Equal(item.Status, ScheduledFeedbackRecordStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledFeedbackRecordMovesToApproved()
    {
        var item = ScheduledFeedbackRecord.NewScheduledFeedbackRecord("code-702", "label-142");
        try
        {
            item.MoveTo(ScheduledFeedbackRecordStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledFeedbackRecordStatus.Approved, "status");
    }

    public static void ScheduledFeedbackRecordRejectsMovesFromWithdrawn()
    {
        var item = ScheduledFeedbackRecord.NewScheduledFeedbackRecord("code-702", "label-142");
        item.Status = ScheduledFeedbackRecordStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ScheduledFeedbackRecordStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ScheduledFeedbackRecordStartsInProposed();
        ScheduledFeedbackRecordMovesToApproved();
        ScheduledFeedbackRecordRejectsMovesFromWithdrawn();
    }
}
