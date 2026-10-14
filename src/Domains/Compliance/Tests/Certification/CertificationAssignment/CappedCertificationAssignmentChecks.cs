namespace Acme.Workforce.Compliance.Tests.Certification.CertificationAssignment;

using Acme.Workforce.Compliance.Certification.CertificationAssignment;

/// <summary>Checks of certification assignment code, run by the project's runner.</summary>
public static class CappedCertificationAssignmentChecks
{
    public static void CappedCertificationAssignmentStartsInCreated()
    {
        var item = CappedCertificationAssignment.NewCappedCertificationAssignment("code-392", "title-360");
        Check.Equal(item.Status, CappedCertificationAssignmentStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedCertificationAssignmentMovesToPublished()
    {
        var item = CappedCertificationAssignment.NewCappedCertificationAssignment("code-392", "title-360");
        try
        {
            item.MoveTo(CappedCertificationAssignmentStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedCertificationAssignmentStatus.Published, "status");
    }

    public static void CappedCertificationAssignmentRejectsMovesFromRetired()
    {
        var item = CappedCertificationAssignment.NewCappedCertificationAssignment("code-392", "title-360");
        item.Status = CappedCertificationAssignmentStatus.Retired;
        Check.Fails(() => item.MoveTo(CappedCertificationAssignmentStatus.Created), "move");
    }

    public static void RunAll()
    {
        CappedCertificationAssignmentStartsInCreated();
        CappedCertificationAssignmentMovesToPublished();
        CappedCertificationAssignmentRejectsMovesFromRetired();
    }
}
