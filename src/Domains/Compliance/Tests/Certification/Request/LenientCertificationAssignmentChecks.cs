namespace Acme.Workforce.Compliance.Tests.Certification.Request;

using Acme.Workforce.Compliance.Certification.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class LenientCertificationAssignmentChecks
{
    public static void LenientCertificationAssignmentStartsInRequested()
    {
        var item = LenientCertificationAssignment.NewLenientCertificationAssignment("code-515", "title-388");
        Check.Equal(item.Status, LenientCertificationAssignmentStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientCertificationAssignmentMovesToGranted()
    {
        var item = LenientCertificationAssignment.NewLenientCertificationAssignment("code-515", "title-388");
        try
        {
            item.MoveTo(LenientCertificationAssignmentStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientCertificationAssignmentStatus.Granted, "status");
    }

    public static void LenientCertificationAssignmentRejectsMovesFromExpired()
    {
        var item = LenientCertificationAssignment.NewLenientCertificationAssignment("code-515", "title-388");
        item.Status = LenientCertificationAssignmentStatus.Expired;
        Check.Fails(() => item.MoveTo(LenientCertificationAssignmentStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LenientCertificationAssignmentStartsInRequested();
        LenientCertificationAssignmentMovesToGranted();
        LenientCertificationAssignmentRejectsMovesFromExpired();
    }
}
