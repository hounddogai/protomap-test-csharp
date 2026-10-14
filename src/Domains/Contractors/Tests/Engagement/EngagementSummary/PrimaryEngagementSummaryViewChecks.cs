namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementSummary;

using Acme.Workforce.Contractors.Engagement.EngagementSummary;

/// <summary>Checks of engagement summary code, run by the project's runner.</summary>
public static class PrimaryEngagementSummaryViewChecks
{
    public static void PrimaryEngagementSummaryViewShowsTheStatusInCapitals()
    {
        var item = PrimaryEngagementSummary.NewPrimaryEngagementSummary("code-255", "title-268");
        var view = PrimaryEngagementSummaryView.ToPrimaryEngagementSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryEngagementSummaryView.ToPrimaryEngagementSummaryViews(new List<PrimaryEngagementSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryEngagementSummaryViewShowsTheStatusInCapitals();
    }
}
