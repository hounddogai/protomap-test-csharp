namespace Acme.Workforce.Workplace.Tests.Visitor.Baseline;

using Acme.Workforce.Workplace.Visitor.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class VisitorBaselineViewChecks
{
    public static void VisitorBaselineViewShowsTheStatusInCapitals()
    {
        var item = VisitorBaseline.NewVisitorBaseline("code-720", "title-31");
        var view = VisitorBaselineView.ToVisitorBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VisitorBaselineView.ToVisitorBaselineViews(new List<VisitorBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VisitorBaselineViewShowsTheStatusInCapitals();
    }
}
