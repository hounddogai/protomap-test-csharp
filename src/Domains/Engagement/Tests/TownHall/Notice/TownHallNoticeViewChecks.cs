namespace Acme.Workforce.Engagement.Tests.TownHall.Notice;

using Acme.Workforce.Engagement.TownHall.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class TownHallNoticeViewChecks
{
    public static void TownHallNoticeViewShowsTheStatusInCapitals()
    {
        var item = TownHallNotice.NewTownHallNotice("code-293", "name-321");
        var view = TownHallNoticeView.ToTownHallNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TownHallNoticeView.ToTownHallNoticeViews(new List<TownHallNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TownHallNoticeViewShowsTheStatusInCapitals();
    }
}
