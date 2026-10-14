namespace Acme.Workforce.Engagement.Tests.TownHall.Window;

using Acme.Workforce.Engagement.TownHall.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TownHallWindowViewChecks
{
    public static void TownHallWindowViewShowsTheStatusInCapitals()
    {
        var item = TownHallWindow.NewTownHallWindow("code-54", "title-666");
        var view = TownHallWindowView.ToTownHallWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TownHallWindowView.ToTownHallWindowViews(new List<TownHallWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TownHallWindowViewShowsTheStatusInCapitals();
    }
}
