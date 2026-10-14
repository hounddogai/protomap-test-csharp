namespace Acme.Workforce.Workplace.Tests.Visitor.Adjustment;

using Acme.Workforce.Workplace.Visitor.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class DefaultVisitorAssignmentViewChecks
{
    public static void DefaultVisitorAssignmentViewShowsTheStatusInCapitals()
    {
        var item = DefaultVisitorAssignment.NewDefaultVisitorAssignment("code-51", "label-180");
        var view = DefaultVisitorAssignmentView.ToDefaultVisitorAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultVisitorAssignmentView.ToDefaultVisitorAssignmentViews(new List<DefaultVisitorAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultVisitorAssignmentViewShowsTheStatusInCapitals();
    }
}
