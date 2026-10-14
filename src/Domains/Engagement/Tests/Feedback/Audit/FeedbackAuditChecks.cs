namespace Acme.Workforce.Engagement.Tests.Feedback.Audit;

using Acme.Workforce.Engagement.Feedback.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FeedbackAuditChecks
{
    public static void FeedbackAuditStartsInIdle()
    {
        var item = FeedbackAudit.NewFeedbackAudit("code-860", "name-812");
        Check.Equal(item.Status, FeedbackAuditStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackAuditMovesToStopped()
    {
        var item = FeedbackAudit.NewFeedbackAudit("code-860", "name-812");
        try
        {
            item.MoveTo(FeedbackAuditStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackAuditStatus.Stopped, "status");
    }

    public static void FeedbackAuditRejectsMovesFromStopped()
    {
        var item = FeedbackAudit.NewFeedbackAudit("code-860", "name-812");
        item.Status = FeedbackAuditStatus.Stopped;
        Check.Fails(() => item.MoveTo(FeedbackAuditStatus.Idle), "move");
    }

    public static void RunAll()
    {
        FeedbackAuditStartsInIdle();
        FeedbackAuditMovesToStopped();
        FeedbackAuditRejectsMovesFromStopped();
    }
}
