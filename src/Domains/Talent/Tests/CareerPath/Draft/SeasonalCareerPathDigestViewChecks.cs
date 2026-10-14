namespace Acme.Workforce.Talent.Tests.CareerPath.Draft;

using Acme.Workforce.Talent.CareerPath.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class SeasonalCareerPathDigestViewChecks
{
    public static void SeasonalCareerPathDigestViewShowsTheStatusInCapitals()
    {
        var item = SeasonalCareerPathDigest.NewSeasonalCareerPathDigest("code-972", "label-577");
        var view = SeasonalCareerPathDigestView.ToSeasonalCareerPathDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalCareerPathDigestView.ToSeasonalCareerPathDigestViews(new List<SeasonalCareerPathDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalCareerPathDigestViewShowsTheStatusInCapitals();
    }
}
