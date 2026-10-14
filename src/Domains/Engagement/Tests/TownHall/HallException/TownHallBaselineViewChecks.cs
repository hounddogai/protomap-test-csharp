namespace Acme.Workforce.Engagement.Tests.TownHall.HallException;

using Acme.Workforce.Engagement.TownHall.HallException;

/// <summary>Checks of hall exception code, run by the project's runner.</summary>
public static class TownHallBaselineViewChecks
{
    public static void TownHallBaselineViewShowsTheStatusInCapitals()
    {
        var item = TownHallBaseline.NewTownHallBaseline("code-809", "name-29");
        var view = TownHallBaselineView.ToTownHallBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TownHallBaselineView.ToTownHallBaselineViews(new List<TownHallBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TownHallBaselineViewShowsTheStatusInCapitals();
    }
}
