namespace Acme.Workforce.Engagement.Tests.TownHall.HallException;

using Acme.Workforce.Engagement.TownHall.HallException;

/// <summary>Checks of hall exception code, run by the project's runner.</summary>
public static class TownHallPreferenceViewChecks
{
    public static void TownHallPreferenceViewShowsTheStatusInCapitals()
    {
        var item = TownHallPreference.NewTownHallPreference("code-388", "label-781");
        var view = TownHallPreferenceView.ToTownHallPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TownHallPreferenceView.ToTownHallPreferenceViews(new List<TownHallPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TownHallPreferenceViewShowsTheStatusInCapitals();
    }
}
