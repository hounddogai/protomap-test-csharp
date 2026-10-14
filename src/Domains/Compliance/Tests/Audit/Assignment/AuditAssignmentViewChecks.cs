namespace Acme.Workforce.Compliance.Tests.Audit.Assignment;

using Acme.Workforce.Compliance.Audit.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class AuditAssignmentViewChecks
{
    public static void AuditAssignmentViewShowsTheStatusInCapitals()
    {
        var item = AuditAssignment.NewAuditAssignment("code-83", "name-795");
        var view = AuditAssignmentView.ToAuditAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AuditAssignmentView.ToAuditAssignmentViews(new List<AuditAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AuditAssignmentViewShowsTheStatusInCapitals();
    }
}
