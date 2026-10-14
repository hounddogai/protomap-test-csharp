namespace Acme.Workforce.Compliance.Tests.Certification.CertificationSummary;

using Acme.Workforce.Compliance.Certification.CertificationSummary;

/// <summary>Checks of certification summary code, run by the project's runner.</summary>
public static class LenientCertificationPlanChecks
{
    public static void LenientCertificationPlanStartsInNew()
    {
        var item = LenientCertificationPlan.NewLenientCertificationPlan("code-128", "label-8");
        Check.Equal(item.Status, LenientCertificationPlanStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientCertificationPlanMovesToDone()
    {
        var item = LenientCertificationPlan.NewLenientCertificationPlan("code-128", "label-8");
        try
        {
            item.MoveTo(LenientCertificationPlanStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientCertificationPlanStatus.Done, "status");
    }

    public static void LenientCertificationPlanRejectsMovesFromFailed()
    {
        var item = LenientCertificationPlan.NewLenientCertificationPlan("code-128", "label-8");
        item.Status = LenientCertificationPlanStatus.Failed;
        Check.Fails(() => item.MoveTo(LenientCertificationPlanStatus.New), "move");
    }

    public static void RunAll()
    {
        LenientCertificationPlanStartsInNew();
        LenientCertificationPlanMovesToDone();
        LenientCertificationPlanRejectsMovesFromFailed();
    }
}
