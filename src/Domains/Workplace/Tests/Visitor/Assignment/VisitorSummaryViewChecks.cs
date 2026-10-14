namespace Acme.Workforce.Workplace.Tests.Visitor.Assignment;

using Acme.Workforce.Workplace.Visitor.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class VisitorSummaryViewChecks
{
    public static void VisitorSummaryViewShowsTheStatusInCapitals()
    {
        var item = VisitorSummary.NewVisitorSummary("code-638", "title-746");
        var view = VisitorSummaryView.ToVisitorSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VisitorSummaryView.ToVisitorSummaryViews(new List<VisitorSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VisitorSummaryViewShowsTheStatusInCapitals();
    }
}
