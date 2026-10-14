namespace Acme.Workforce.Compliance.Tests.Certification.Slot;

using Acme.Workforce.Compliance.Certification.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class SeasonalCertificationManifestViewChecks
{
    public static void SeasonalCertificationManifestViewShowsTheStatusInCapitals()
    {
        var item = SeasonalCertificationManifest.NewSeasonalCertificationManifest("code-271", "label-448");
        var view = SeasonalCertificationManifestView.ToSeasonalCertificationManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalCertificationManifestView.ToSeasonalCertificationManifestViews(new List<SeasonalCertificationManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalCertificationManifestViewShowsTheStatusInCapitals();
    }
}
