namespace Acme.Workforce.Orgdesign.Tests.Position.Plan;

using Acme.Workforce.Orgdesign.Position.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RollingPositionManifestViewChecks
{
    public static void RollingPositionManifestViewShowsTheStatusInCapitals()
    {
        var item = RollingPositionManifest.NewRollingPositionManifest("code-469", "name-133");
        var view = RollingPositionManifestView.ToRollingPositionManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingPositionManifestView.ToRollingPositionManifestViews(new List<RollingPositionManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingPositionManifestViewShowsTheStatusInCapitals();
    }
}
