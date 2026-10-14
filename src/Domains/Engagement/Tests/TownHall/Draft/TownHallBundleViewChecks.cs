namespace Acme.Workforce.Engagement.Tests.TownHall.Draft;

using Acme.Workforce.Engagement.TownHall.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class TownHallBundleViewChecks
{
    public static void TownHallBundleViewShowsTheStatusInCapitals()
    {
        var item = TownHallBundle.NewTownHallBundle("code-452", "name-546");
        var view = TownHallBundleView.ToTownHallBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TownHallBundleView.ToTownHallBundleViews(new List<TownHallBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TownHallBundleViewShowsTheStatusInCapitals();
    }
}
