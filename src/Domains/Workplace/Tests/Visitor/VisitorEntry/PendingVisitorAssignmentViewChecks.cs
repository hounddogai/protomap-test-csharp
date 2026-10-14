namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorEntry;

using Acme.Workforce.Workplace.Visitor.VisitorEntry;

/// <summary>Checks of visitor entry code, run by the project's runner.</summary>
public static class PendingVisitorAssignmentViewChecks
{
    public static void PendingVisitorAssignmentViewShowsTheStatusInCapitals()
    {
        var item = PendingVisitorAssignment.NewPendingVisitorAssignment("code-185", "label-951");
        var view = PendingVisitorAssignmentView.ToPendingVisitorAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingVisitorAssignmentView.ToPendingVisitorAssignmentViews(new List<PendingVisitorAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingVisitorAssignmentViewShowsTheStatusInCapitals();
    }
}
