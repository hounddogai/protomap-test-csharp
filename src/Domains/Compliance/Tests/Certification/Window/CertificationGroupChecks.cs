namespace Acme.Workforce.Compliance.Tests.Certification.Window;

using Acme.Workforce.Compliance.Certification.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class CertificationGroupChecks
{
    public static void CertificationGroupStartsInPlanned()
    {
        var item = CertificationGroup.NewCertificationGroup("code-9", "name-841");
        Check.Equal(item.Status, CertificationGroupStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CertificationGroupMovesToCompleted()
    {
        var item = CertificationGroup.NewCertificationGroup("code-9", "name-841");
        try
        {
            item.MoveTo(CertificationGroupStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CertificationGroupStatus.Completed, "status");
    }

    public static void CertificationGroupRejectsMovesFromCancelled()
    {
        var item = CertificationGroup.NewCertificationGroup("code-9", "name-841");
        item.Status = CertificationGroupStatus.Cancelled;
        Check.Fails(() => item.MoveTo(CertificationGroupStatus.Planned), "move");
    }

    public static void RunAll()
    {
        CertificationGroupStartsInPlanned();
        CertificationGroupMovesToCompleted();
        CertificationGroupRejectsMovesFromCancelled();
    }
}
