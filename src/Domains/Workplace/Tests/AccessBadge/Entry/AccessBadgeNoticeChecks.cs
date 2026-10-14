namespace Acme.Workforce.Workplace.Tests.AccessBadge.Entry;

using Acme.Workforce.Workplace.AccessBadge.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AccessBadgeNoticeChecks
{
    public static void AccessBadgeNoticeStartsInDraft()
    {
        var item = AccessBadgeNotice.NewAccessBadgeNotice("code-850", "title-260");
        Check.Equal(item.Status, AccessBadgeNoticeStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccessBadgeNoticeMovesToSuspended()
    {
        var item = AccessBadgeNotice.NewAccessBadgeNotice("code-850", "title-260");
        try
        {
            item.MoveTo(AccessBadgeNoticeStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccessBadgeNoticeStatus.Suspended, "status");
    }

    public static void AccessBadgeNoticeRejectsMovesFromClosed()
    {
        var item = AccessBadgeNotice.NewAccessBadgeNotice("code-850", "title-260");
        item.Status = AccessBadgeNoticeStatus.Closed;
        Check.Fails(() => item.MoveTo(AccessBadgeNoticeStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AccessBadgeNoticeStartsInDraft();
        AccessBadgeNoticeMovesToSuspended();
        AccessBadgeNoticeRejectsMovesFromClosed();
    }
}
