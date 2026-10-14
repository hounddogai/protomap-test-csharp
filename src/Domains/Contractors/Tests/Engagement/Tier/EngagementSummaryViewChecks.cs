namespace Acme.Workforce.Contractors.Tests.Engagement.Tier;

using Acme.Workforce.Contractors.Engagement.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class EngagementSummaryViewChecks
{
    public static void EngagementSummaryViewShowsTheStatusInCapitals()
    {
        var item = EngagementSummary.NewEngagementSummary("code-595", "label-187");
        var view = EngagementSummaryView.ToEngagementSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EngagementSummaryView.ToEngagementSummaryViews(new List<EngagementSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EngagementSummaryViewShowsTheStatusInCapitals();
    }
}
