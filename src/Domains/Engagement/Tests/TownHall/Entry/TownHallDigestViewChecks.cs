namespace Acme.Workforce.Engagement.Tests.TownHall.Entry;

using Acme.Workforce.Engagement.TownHall.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class TownHallDigestViewChecks
{
    public static void TownHallDigestViewShowsTheStatusInCapitals()
    {
        var item = TownHallDigest.NewTownHallDigest("code-57", "label-655");
        var view = TownHallDigestView.ToTownHallDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TownHallDigestView.ToTownHallDigestViews(new List<TownHallDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TownHallDigestViewShowsTheStatusInCapitals();
    }
}
