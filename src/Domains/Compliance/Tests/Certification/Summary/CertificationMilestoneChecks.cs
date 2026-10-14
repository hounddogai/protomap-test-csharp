namespace Acme.Workforce.Compliance.Tests.Certification.Summary;

using Acme.Workforce.Compliance.Certification.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class CertificationMilestoneChecks
{
    public static void CertificationMilestoneStartsInCreated()
    {
        var item = CertificationMilestone.NewCertificationMilestone("code-557", "title-371");
        Check.Equal(item.Status, CertificationMilestoneStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CertificationMilestoneMovesToPublished()
    {
        var item = CertificationMilestone.NewCertificationMilestone("code-557", "title-371");
        try
        {
            item.MoveTo(CertificationMilestoneStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CertificationMilestoneStatus.Published, "status");
    }

    public static void CertificationMilestoneRejectsMovesFromRetired()
    {
        var item = CertificationMilestone.NewCertificationMilestone("code-557", "title-371");
        item.Status = CertificationMilestoneStatus.Retired;
        Check.Fails(() => item.MoveTo(CertificationMilestoneStatus.Created), "move");
    }

    public static void RunAll()
    {
        CertificationMilestoneStartsInCreated();
        CertificationMilestoneMovesToPublished();
        CertificationMilestoneRejectsMovesFromRetired();
    }
}
