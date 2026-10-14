namespace Acme.Workforce.Compliance.Tests.Certification.Assignment;

using Acme.Workforce.Compliance.Certification.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class CertificationAssignmentViewChecks
{
    public static void CertificationAssignmentViewShowsTheStatusInCapitals()
    {
        var item = CertificationAssignment.NewCertificationAssignment("code-979", "label-279");
        var view = CertificationAssignmentView.ToCertificationAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CertificationAssignmentView.ToCertificationAssignmentViews(new List<CertificationAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CertificationAssignmentViewShowsTheStatusInCapitals();
    }
}
