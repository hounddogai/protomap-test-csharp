namespace Acme.Workforce.Contractors.Tests.Engagement.Target;

using Acme.Workforce.Contractors.Engagement.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class SeasonalEngagementMilestoneViewChecks
{
    public static void SeasonalEngagementMilestoneViewShowsTheStatusInCapitals()
    {
        var item = SeasonalEngagementMilestone.NewSeasonalEngagementMilestone("code-958", "label-100");
        var view = SeasonalEngagementMilestoneView.ToSeasonalEngagementMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalEngagementMilestoneView.ToSeasonalEngagementMilestoneViews(new List<SeasonalEngagementMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalEngagementMilestoneViewShowsTheStatusInCapitals();
    }
}
