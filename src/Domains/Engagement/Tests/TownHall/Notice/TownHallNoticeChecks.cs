namespace Acme.Workforce.Engagement.Tests.TownHall.Notice;

using Acme.Workforce.Engagement.TownHall.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class TownHallNoticeChecks
{
    public static void TownHallNoticeStartsInOpen()
    {
        var item = TownHallNotice.NewTownHallNotice("code-293", "name-321");
        Check.Equal(item.Status, TownHallNoticeStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TownHallNoticeMovesToInReview()
    {
        var item = TownHallNotice.NewTownHallNotice("code-293", "name-321");
        try
        {
            item.MoveTo(TownHallNoticeStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TownHallNoticeStatus.InReview, "status");
    }

    public static void TownHallNoticeRejectsMovesFromArchived()
    {
        var item = TownHallNotice.NewTownHallNotice("code-293", "name-321");
        item.Status = TownHallNoticeStatus.Archived;
        Check.Fails(() => item.MoveTo(TownHallNoticeStatus.Open), "move");
    }

    public static void RunAll()
    {
        TownHallNoticeStartsInOpen();
        TownHallNoticeMovesToInReview();
        TownHallNoticeRejectsMovesFromArchived();
    }
}
