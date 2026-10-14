namespace Acme.Workforce.Workplace.Tests.Visitor.Proposal;

using Acme.Workforce.Workplace.Visitor.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class VisitorMappingViewChecks
{
    public static void VisitorMappingViewShowsTheStatusInCapitals()
    {
        var item = VisitorMapping.NewVisitorMapping("code-364", "name-304");
        var view = VisitorMappingView.ToVisitorMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VisitorMappingView.ToVisitorMappingViews(new List<VisitorMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VisitorMappingViewShowsTheStatusInCapitals();
    }
}
