namespace Acme.Workforce.Compliance.Tests.Certification.Assignment;

using Acme.Workforce.Compliance.Certification.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class CertificationAssignmentChecks
{
    public static void CertificationAssignmentStartsInIdle()
    {
        var item = CertificationAssignment.NewCertificationAssignment("code-979", "label-279");
        Check.Equal(item.Status, CertificationAssignmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CertificationAssignmentMovesToReady()
    {
        var item = CertificationAssignment.NewCertificationAssignment("code-979", "label-279");
        try
        {
            item.MoveTo(CertificationAssignmentStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CertificationAssignmentStatus.Ready, "status");
    }

    public static void CertificationAssignmentRejectsMovesFromStopped()
    {
        var item = CertificationAssignment.NewCertificationAssignment("code-979", "label-279");
        item.Status = CertificationAssignmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(CertificationAssignmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        CertificationAssignmentStartsInIdle();
        CertificationAssignmentMovesToReady();
        CertificationAssignmentRejectsMovesFromStopped();
    }
}
