namespace Acme.Workforce.Engagement.Tests.TownHall.Ledger;

using Acme.Workforce.Engagement.TownHall.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class AdaptiveTownHallNoticeViewChecks
{
    public static void AdaptiveTownHallNoticeViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveTownHallNotice.NewAdaptiveTownHallNotice("code-612", "title-436");
        var view = AdaptiveTownHallNoticeView.ToAdaptiveTownHallNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveTownHallNoticeView.ToAdaptiveTownHallNoticeViews(new List<AdaptiveTownHallNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveTownHallNoticeViewShowsTheStatusInCapitals();
    }
}
