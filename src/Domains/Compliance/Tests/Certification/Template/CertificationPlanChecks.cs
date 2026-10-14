namespace Acme.Workforce.Compliance.Tests.Certification.Template;

using Acme.Workforce.Compliance.Certification.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class CertificationPlanChecks
{
    public static void CertificationPlanStartsInPlanned()
    {
        var item = CertificationPlan.NewCertificationPlan("code-465", "name-549");
        Check.Equal(item.Status, CertificationPlanStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CertificationPlanMovesToScheduled()
    {
        var item = CertificationPlan.NewCertificationPlan("code-465", "name-549");
        try
        {
            item.MoveTo(CertificationPlanStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CertificationPlanStatus.Scheduled, "status");
    }

    public static void CertificationPlanRejectsMovesFromCancelled()
    {
        var item = CertificationPlan.NewCertificationPlan("code-465", "name-549");
        item.Status = CertificationPlanStatus.Cancelled;
        Check.Fails(() => item.MoveTo(CertificationPlanStatus.Planned), "move");
    }

    public static void RunAll()
    {
        CertificationPlanStartsInPlanned();
        CertificationPlanMovesToScheduled();
        CertificationPlanRejectsMovesFromCancelled();
    }
}
