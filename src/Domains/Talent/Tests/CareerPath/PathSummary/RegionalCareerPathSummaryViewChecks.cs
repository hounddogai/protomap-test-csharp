namespace Acme.Workforce.Talent.Tests.CareerPath.PathSummary;

using Acme.Workforce.Talent.CareerPath.PathSummary;

/// <summary>Checks of path summary code, run by the project's runner.</summary>
public static class RegionalCareerPathSummaryViewChecks
{
    public static void RegionalCareerPathSummaryViewShowsTheStatusInCapitals()
    {
        var item = RegionalCareerPathSummary.NewRegionalCareerPathSummary("code-498", "title-732");
        var view = RegionalCareerPathSummaryView.ToRegionalCareerPathSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalCareerPathSummaryView.ToRegionalCareerPathSummaryViews(new List<RegionalCareerPathSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalCareerPathSummaryViewShowsTheStatusInCapitals();
    }
}
