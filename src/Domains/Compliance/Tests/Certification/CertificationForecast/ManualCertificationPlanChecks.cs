namespace Acme.Workforce.Compliance.Tests.Certification.CertificationForecast;

using Acme.Workforce.Compliance.Certification.CertificationForecast;

/// <summary>Checks of certification forecast code, run by the project's runner.</summary>
public static class ManualCertificationPlanChecks
{
    public static void ManualCertificationPlanStartsInCreated()
    {
        var item = ManualCertificationPlan.NewManualCertificationPlan("code-689", "name-20");
        Check.Equal(item.Status, ManualCertificationPlanStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualCertificationPlanMovesToRetired()
    {
        var item = ManualCertificationPlan.NewManualCertificationPlan("code-689", "name-20");
        try
        {
            item.MoveTo(ManualCertificationPlanStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualCertificationPlanStatus.Retired, "status");
    }

    public static void ManualCertificationPlanRejectsMovesFromRetired()
    {
        var item = ManualCertificationPlan.NewManualCertificationPlan("code-689", "name-20");
        item.Status = ManualCertificationPlanStatus.Retired;
        Check.Fails(() => item.MoveTo(ManualCertificationPlanStatus.Created), "move");
    }

    public static void RunAll()
    {
        ManualCertificationPlanStartsInCreated();
        ManualCertificationPlanMovesToRetired();
        ManualCertificationPlanRejectsMovesFromRetired();
    }
}
