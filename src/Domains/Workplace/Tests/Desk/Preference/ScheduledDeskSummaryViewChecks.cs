namespace Acme.Workforce.Workplace.Tests.Desk.Preference;

using Acme.Workforce.Workplace.Desk.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ScheduledDeskSummaryViewChecks
{
    public static void ScheduledDeskSummaryViewShowsTheStatusInCapitals()
    {
        var item = ScheduledDeskSummary.NewScheduledDeskSummary("code-698", "title-469");
        var view = ScheduledDeskSummaryView.ToScheduledDeskSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledDeskSummaryView.ToScheduledDeskSummaryViews(new List<ScheduledDeskSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledDeskSummaryViewShowsTheStatusInCapitals();
    }
}
