namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Policy;

using Acme.Workforce.Talent.SuccessionPlan.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class RegionalSuccessionPlanAgreementChecks
{
    public static void RegionalSuccessionPlanAgreementStartsInIdle()
    {
        var item = RegionalSuccessionPlanAgreement.NewRegionalSuccessionPlanAgreement("code-421", "label-181");
        Check.Equal(item.Status, RegionalSuccessionPlanAgreementStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalSuccessionPlanAgreementMovesToDraining()
    {
        var item = RegionalSuccessionPlanAgreement.NewRegionalSuccessionPlanAgreement("code-421", "label-181");
        try
        {
            item.MoveTo(RegionalSuccessionPlanAgreementStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalSuccessionPlanAgreementStatus.Draining, "status");
    }

    public static void RegionalSuccessionPlanAgreementRejectsMovesFromStopped()
    {
        var item = RegionalSuccessionPlanAgreement.NewRegionalSuccessionPlanAgreement("code-421", "label-181");
        item.Status = RegionalSuccessionPlanAgreementStatus.Stopped;
        Check.Fails(() => item.MoveTo(RegionalSuccessionPlanAgreementStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RegionalSuccessionPlanAgreementStartsInIdle();
        RegionalSuccessionPlanAgreementMovesToDraining();
        RegionalSuccessionPlanAgreementRejectsMovesFromStopped();
    }
}
