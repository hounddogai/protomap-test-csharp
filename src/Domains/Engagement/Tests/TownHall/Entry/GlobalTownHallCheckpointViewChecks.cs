namespace Acme.Workforce.Engagement.Tests.TownHall.Entry;

using Acme.Workforce.Engagement.TownHall.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class GlobalTownHallCheckpointViewChecks
{
    public static void GlobalTownHallCheckpointViewShowsTheStatusInCapitals()
    {
        var item = GlobalTownHallCheckpoint.NewGlobalTownHallCheckpoint("code-933", "label-190");
        var view = GlobalTownHallCheckpointView.ToGlobalTownHallCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalTownHallCheckpointView.ToGlobalTownHallCheckpointViews(new List<GlobalTownHallCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalTownHallCheckpointViewShowsTheStatusInCapitals();
    }
}
