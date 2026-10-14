namespace Acme.Workforce.Assets.Tests.Refresh.Checkpoint;

using Acme.Workforce.Assets.Refresh.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class RefreshNoticeChecks
{
    public static void RefreshNoticeStartsInCreated()
    {
        var item = RefreshNotice.NewRefreshNotice("code-371", "name-26");
        Check.Equal(item.Status, RefreshNoticeStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RefreshNoticeMovesToPublished()
    {
        var item = RefreshNotice.NewRefreshNotice("code-371", "name-26");
        try
        {
            item.MoveTo(RefreshNoticeStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RefreshNoticeStatus.Published, "status");
    }

    public static void RefreshNoticeRejectsMovesFromRetired()
    {
        var item = RefreshNotice.NewRefreshNotice("code-371", "name-26");
        item.Status = RefreshNoticeStatus.Retired;
        Check.Fails(() => item.MoveTo(RefreshNoticeStatus.Created), "move");
    }

    public static void RunAll()
    {
        RefreshNoticeStartsInCreated();
        RefreshNoticeMovesToPublished();
        RefreshNoticeRejectsMovesFromRetired();
    }
}
