namespace Acme.Workforce.Contractors.Tests.Engagement.Notice;

using Acme.Workforce.Contractors.Engagement.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class SeasonalEngagementRollupViewChecks
{
    public static void SeasonalEngagementRollupViewShowsTheStatusInCapitals()
    {
        var item = SeasonalEngagementRollup.NewSeasonalEngagementRollup("code-601", "title-56");
        var view = SeasonalEngagementRollupView.ToSeasonalEngagementRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalEngagementRollupView.ToSeasonalEngagementRollupViews(new List<SeasonalEngagementRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalEngagementRollupViewShowsTheStatusInCapitals();
    }
}
