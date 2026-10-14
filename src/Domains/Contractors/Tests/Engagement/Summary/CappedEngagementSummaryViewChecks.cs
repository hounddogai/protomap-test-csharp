namespace Acme.Workforce.Contractors.Tests.Engagement.Summary;

using Acme.Workforce.Contractors.Engagement.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class CappedEngagementSummaryViewChecks
{
    public static void CappedEngagementSummaryViewShowsTheStatusInCapitals()
    {
        var item = CappedEngagementSummary.NewCappedEngagementSummary("code-89", "label-840");
        var view = CappedEngagementSummaryView.ToCappedEngagementSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedEngagementSummaryView.ToCappedEngagementSummaryViews(new List<CappedEngagementSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedEngagementSummaryViewShowsTheStatusInCapitals();
    }
}
