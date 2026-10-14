namespace Acme.Workforce.Compliance.Tests.Certification.Digest;

using Acme.Workforce.Compliance.Certification.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class SeasonalCertificationDigestViewChecks
{
    public static void SeasonalCertificationDigestViewShowsTheStatusInCapitals()
    {
        var item = SeasonalCertificationDigest.NewSeasonalCertificationDigest("code-15", "label-264");
        var view = SeasonalCertificationDigestView.ToSeasonalCertificationDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalCertificationDigestView.ToSeasonalCertificationDigestViews(new List<SeasonalCertificationDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalCertificationDigestViewShowsTheStatusInCapitals();
    }
}
