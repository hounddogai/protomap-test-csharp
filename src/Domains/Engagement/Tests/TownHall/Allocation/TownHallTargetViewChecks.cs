namespace Acme.Workforce.Engagement.Tests.TownHall.Allocation;

using Acme.Workforce.Engagement.TownHall.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class TownHallTargetViewChecks
{
    public static void TownHallTargetViewShowsTheStatusInCapitals()
    {
        var item = TownHallTarget.NewTownHallTarget("code-734", "title-560");
        var view = TownHallTargetView.ToTownHallTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TownHallTargetView.ToTownHallTargetViews(new List<TownHallTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TownHallTargetViewShowsTheStatusInCapitals();
    }
}
