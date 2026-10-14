namespace Acme.Workforce.Engagement.Tests.TownHall.Threshold;

using Acme.Workforce.Engagement.TownHall.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LegacyTownHallCheckpointViewChecks
{
    public static void LegacyTownHallCheckpointViewShowsTheStatusInCapitals()
    {
        var item = LegacyTownHallCheckpoint.NewLegacyTownHallCheckpoint("code-168", "label-406");
        var view = LegacyTownHallCheckpointView.ToLegacyTownHallCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyTownHallCheckpointView.ToLegacyTownHallCheckpointViews(new List<LegacyTownHallCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyTownHallCheckpointViewShowsTheStatusInCapitals();
    }
}
