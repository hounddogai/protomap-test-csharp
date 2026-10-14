namespace Acme.Workforce.Engagement.Tests.TownHall.OverrideItem;

using Acme.Workforce.Engagement.TownHall.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class TownHallOverrideViewChecks
{
    public static void TownHallOverrideViewShowsTheStatusInCapitals()
    {
        var item = TownHallOverride.NewTownHallOverride("code-520", "title-865");
        var view = TownHallOverrideView.ToTownHallOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TownHallOverrideView.ToTownHallOverrideViews(new List<TownHallOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TownHallOverrideViewShowsTheStatusInCapitals();
    }
}
