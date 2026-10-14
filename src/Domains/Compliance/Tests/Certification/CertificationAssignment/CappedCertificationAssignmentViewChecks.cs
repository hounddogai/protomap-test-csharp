namespace Acme.Workforce.Compliance.Tests.Certification.CertificationAssignment;

using Acme.Workforce.Compliance.Certification.CertificationAssignment;

/// <summary>Checks of certification assignment code, run by the project's runner.</summary>
public static class CappedCertificationAssignmentViewChecks
{
    public static void CappedCertificationAssignmentViewShowsTheStatusInCapitals()
    {
        var item = CappedCertificationAssignment.NewCappedCertificationAssignment("code-392", "title-360");
        var view = CappedCertificationAssignmentView.ToCappedCertificationAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedCertificationAssignmentView.ToCappedCertificationAssignmentViews(new List<CappedCertificationAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedCertificationAssignmentViewShowsTheStatusInCapitals();
    }
}
