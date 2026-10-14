namespace Acme.Workforce.Workplace.Tests.AccessBadge.Notice;

using Acme.Workforce.Workplace.AccessBadge.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class ManualAccessBadgeNoticeChecks
{
    public static void ManualAccessBadgeNoticeStartsInPlanned()
    {
        var item = ManualAccessBadgeNotice.NewManualAccessBadgeNotice("code-998", "name-643");
        Check.Equal(item.Status, ManualAccessBadgeNoticeStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualAccessBadgeNoticeMovesToCancelled()
    {
        var item = ManualAccessBadgeNotice.NewManualAccessBadgeNotice("code-998", "name-643");
        try
        {
            item.MoveTo(ManualAccessBadgeNoticeStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualAccessBadgeNoticeStatus.Cancelled, "status");
    }

    public static void ManualAccessBadgeNoticeRejectsMovesFromCancelled()
    {
        var item = ManualAccessBadgeNotice.NewManualAccessBadgeNotice("code-998", "name-643");
        item.Status = ManualAccessBadgeNoticeStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ManualAccessBadgeNoticeStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ManualAccessBadgeNoticeStartsInPlanned();
        ManualAccessBadgeNoticeMovesToCancelled();
        ManualAccessBadgeNoticeRejectsMovesFromCancelled();
    }
}
