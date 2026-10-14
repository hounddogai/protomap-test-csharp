namespace Acme.Workforce.Workplace.Tests.Desk.Digest;

using Acme.Workforce.Workplace.Desk.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class DeskSummaryViewChecks
{
    public static void DeskSummaryViewShowsTheStatusInCapitals()
    {
        var item = DeskSummary.NewDeskSummary("code-652", "label-720");
        var view = DeskSummaryView.ToDeskSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeskSummaryView.ToDeskSummaryViews(new List<DeskSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeskSummaryViewShowsTheStatusInCapitals();
    }
}
