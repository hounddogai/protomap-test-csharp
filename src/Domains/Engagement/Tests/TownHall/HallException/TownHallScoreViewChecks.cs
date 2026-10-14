namespace Acme.Workforce.Engagement.Tests.TownHall.HallException;

using Acme.Workforce.Engagement.TownHall.HallException;

/// <summary>Checks of hall exception code, run by the project's runner.</summary>
public static class TownHallScoreViewChecks
{
    public static void TownHallScoreViewShowsTheStatusInCapitals()
    {
        var item = TownHallScore.NewTownHallScore("code-231", "name-473");
        var view = TownHallScoreView.ToTownHallScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TownHallScoreView.ToTownHallScoreViews(new List<TownHallScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TownHallScoreViewShowsTheStatusInCapitals();
    }
}
