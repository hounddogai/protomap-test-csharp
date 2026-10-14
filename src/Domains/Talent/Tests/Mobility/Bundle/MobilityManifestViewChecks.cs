namespace Acme.Workforce.Talent.Tests.Mobility.Bundle;

using Acme.Workforce.Talent.Mobility.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class MobilityManifestViewChecks
{
    public static void MobilityManifestViewShowsTheStatusInCapitals()
    {
        var item = MobilityManifest.NewMobilityManifest("code-55", "name-247");
        var view = MobilityManifestView.ToMobilityManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MobilityManifestView.ToMobilityManifestViews(new List<MobilityManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MobilityManifestViewShowsTheStatusInCapitals();
    }
}
