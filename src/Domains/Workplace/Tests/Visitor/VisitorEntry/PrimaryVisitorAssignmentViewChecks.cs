namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorEntry;

using Acme.Workforce.Workplace.Visitor.VisitorEntry;

/// <summary>Checks of visitor entry code, run by the project's runner.</summary>
public static class PrimaryVisitorAssignmentViewChecks
{
    public static void PrimaryVisitorAssignmentViewShowsTheStatusInCapitals()
    {
        var item = PrimaryVisitorAssignment.NewPrimaryVisitorAssignment("code-991", "label-381");
        var view = PrimaryVisitorAssignmentView.ToPrimaryVisitorAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryVisitorAssignmentView.ToPrimaryVisitorAssignmentViews(new List<PrimaryVisitorAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryVisitorAssignmentViewShowsTheStatusInCapitals();
    }
}
