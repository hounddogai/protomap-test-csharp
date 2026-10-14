namespace Acme.Workforce.Engagement.Tests.TownHall.Draft;

using Acme.Workforce.Engagement.TownHall.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LegacyTownHallManifestViewChecks
{
    public static void LegacyTownHallManifestViewShowsTheStatusInCapitals()
    {
        var item = LegacyTownHallManifest.NewLegacyTownHallManifest("code-312", "name-306");
        var view = LegacyTownHallManifestView.ToLegacyTownHallManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyTownHallManifestView.ToLegacyTownHallManifestViews(new List<LegacyTownHallManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyTownHallManifestViewShowsTheStatusInCapitals();
    }
}
