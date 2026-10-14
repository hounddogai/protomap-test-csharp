namespace Acme.Workforce.Contractors.Tests.Engagement.Digest;

using Acme.Workforce.Contractors.Engagement.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class ScheduledEngagementNoticeChecks
{
    public static void ScheduledEngagementNoticeStartsInDraft()
    {
        var item = ScheduledEngagementNotice.NewScheduledEngagementNotice("code-50", "name-64");
        Check.Equal(item.Status, ScheduledEngagementNoticeStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledEngagementNoticeMovesToSuspended()
    {
        var item = ScheduledEngagementNotice.NewScheduledEngagementNotice("code-50", "name-64");
        try
        {
            item.MoveTo(ScheduledEngagementNoticeStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledEngagementNoticeStatus.Suspended, "status");
    }

    public static void ScheduledEngagementNoticeRejectsMovesFromClosed()
    {
        var item = ScheduledEngagementNotice.NewScheduledEngagementNotice("code-50", "name-64");
        item.Status = ScheduledEngagementNoticeStatus.Closed;
        Check.Fails(() => item.MoveTo(ScheduledEngagementNoticeStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ScheduledEngagementNoticeStartsInDraft();
        ScheduledEngagementNoticeMovesToSuspended();
        ScheduledEngagementNoticeRejectsMovesFromClosed();
    }
}
