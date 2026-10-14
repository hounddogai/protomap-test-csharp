namespace Acme.Workforce.Engagement.Tests.TownHall.Milestone;

using Acme.Workforce.Engagement.TownHall.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class WeightedTownHallManifestViewChecks
{
    public static void WeightedTownHallManifestViewShowsTheStatusInCapitals()
    {
        var item = WeightedTownHallManifest.NewWeightedTownHallManifest("code-746", "name-118");
        var view = WeightedTownHallManifestView.ToWeightedTownHallManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedTownHallManifestView.ToWeightedTownHallManifestViews(new List<WeightedTownHallManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedTownHallManifestViewShowsTheStatusInCapitals();
    }
}
