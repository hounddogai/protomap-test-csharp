namespace Acme.Workforce.Engagement.Tests.TownHall.Exception;

using Acme.Workforce.Engagement.TownHall.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class TownHallCheckpointViewChecks
{
    public static void TownHallCheckpointViewShowsTheStatusInCapitals()
    {
        var item = TownHallCheckpoint.NewTownHallCheckpoint("code-269", "title-25");
        var view = TownHallCheckpointView.ToTownHallCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TownHallCheckpointView.ToTownHallCheckpointViews(new List<TownHallCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TownHallCheckpointViewShowsTheStatusInCapitals();
    }
}
