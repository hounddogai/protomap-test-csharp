namespace Acme.Workforce.Engagement.Tests.TownHall.Notice;

using Acme.Workforce.Engagement.TownHall.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class AdaptiveTownHallManifestViewChecks
{
    public static void AdaptiveTownHallManifestViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveTownHallManifest.NewAdaptiveTownHallManifest("code-25", "name-668");
        var view = AdaptiveTownHallManifestView.ToAdaptiveTownHallManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveTownHallManifestView.ToAdaptiveTownHallManifestViews(new List<AdaptiveTownHallManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveTownHallManifestViewShowsTheStatusInCapitals();
    }
}
