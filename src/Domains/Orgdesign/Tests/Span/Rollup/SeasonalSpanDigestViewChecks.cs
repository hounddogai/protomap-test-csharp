namespace Acme.Workforce.Orgdesign.Tests.Span.Rollup;

using Acme.Workforce.Orgdesign.Span.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SeasonalSpanDigestViewChecks
{
    public static void SeasonalSpanDigestViewShowsTheStatusInCapitals()
    {
        var item = SeasonalSpanDigest.NewSeasonalSpanDigest("code-5", "name-717");
        var view = SeasonalSpanDigestView.ToSeasonalSpanDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalSpanDigestView.ToSeasonalSpanDigestViews(new List<SeasonalSpanDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalSpanDigestViewShowsTheStatusInCapitals();
    }
}
