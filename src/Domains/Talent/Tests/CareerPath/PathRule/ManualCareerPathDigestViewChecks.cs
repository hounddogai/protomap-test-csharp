namespace Acme.Workforce.Talent.Tests.CareerPath.PathRule;

using Acme.Workforce.Talent.CareerPath.PathRule;

/// <summary>Checks of path rule code, run by the project's runner.</summary>
public static class ManualCareerPathDigestViewChecks
{
    public static void ManualCareerPathDigestViewShowsTheStatusInCapitals()
    {
        var item = ManualCareerPathDigest.NewManualCareerPathDigest("code-966", "label-8");
        var view = ManualCareerPathDigestView.ToManualCareerPathDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualCareerPathDigestView.ToManualCareerPathDigestViews(new List<ManualCareerPathDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualCareerPathDigestViewShowsTheStatusInCapitals();
    }
}
