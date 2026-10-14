namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeEntry;

using Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

/// <summary>Checks of badge entry code, run by the project's runner.</summary>
public static class RegionalAccessBadgeScoreViewChecks
{
    public static void RegionalAccessBadgeScoreViewShowsTheStatusInCapitals()
    {
        var item = RegionalAccessBadgeScore.NewRegionalAccessBadgeScore("code-700", "name-363");
        var view = RegionalAccessBadgeScoreView.ToRegionalAccessBadgeScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalAccessBadgeScoreView.ToRegionalAccessBadgeScoreViews(new List<RegionalAccessBadgeScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalAccessBadgeScoreViewShowsTheStatusInCapitals();
    }
}
