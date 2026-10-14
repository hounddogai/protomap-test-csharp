namespace Acme.Workforce.Engagement.Tests.TownHall.Ledger;

using Acme.Workforce.Engagement.TownHall.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class AdaptiveTownHallNoticeChecks
{
    public static void AdaptiveTownHallNoticeStartsInOpen()
    {
        var item = AdaptiveTownHallNotice.NewAdaptiveTownHallNotice("code-612", "title-436");
        Check.Equal(item.Status, AdaptiveTownHallNoticeStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveTownHallNoticeMovesToReopened()
    {
        var item = AdaptiveTownHallNotice.NewAdaptiveTownHallNotice("code-612", "title-436");
        try
        {
            item.MoveTo(AdaptiveTownHallNoticeStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveTownHallNoticeStatus.Reopened, "status");
    }

    public static void AdaptiveTownHallNoticeRejectsMovesFromArchived()
    {
        var item = AdaptiveTownHallNotice.NewAdaptiveTownHallNotice("code-612", "title-436");
        item.Status = AdaptiveTownHallNoticeStatus.Archived;
        Check.Fails(() => item.MoveTo(AdaptiveTownHallNoticeStatus.Open), "move");
    }

    public static void RunAll()
    {
        AdaptiveTownHallNoticeStartsInOpen();
        AdaptiveTownHallNoticeMovesToReopened();
        AdaptiveTownHallNoticeRejectsMovesFromArchived();
    }
}
