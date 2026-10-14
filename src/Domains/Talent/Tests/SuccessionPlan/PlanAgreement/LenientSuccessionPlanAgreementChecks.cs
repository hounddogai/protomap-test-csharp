namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanAgreement;

using Acme.Workforce.Talent.SuccessionPlan.PlanAgreement;

/// <summary>Checks of plan agreement code, run by the project's runner.</summary>
public static class LenientSuccessionPlanAgreementChecks
{
    public static void LenientSuccessionPlanAgreementStartsInCreated()
    {
        var item = LenientSuccessionPlanAgreement.NewLenientSuccessionPlanAgreement("code-692", "name-315");
        Check.Equal(item.Status, LenientSuccessionPlanAgreementStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientSuccessionPlanAgreementMovesToValidated()
    {
        var item = LenientSuccessionPlanAgreement.NewLenientSuccessionPlanAgreement("code-692", "name-315");
        try
        {
            item.MoveTo(LenientSuccessionPlanAgreementStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientSuccessionPlanAgreementStatus.Validated, "status");
    }

    public static void LenientSuccessionPlanAgreementRejectsMovesFromRetired()
    {
        var item = LenientSuccessionPlanAgreement.NewLenientSuccessionPlanAgreement("code-692", "name-315");
        item.Status = LenientSuccessionPlanAgreementStatus.Retired;
        Check.Fails(() => item.MoveTo(LenientSuccessionPlanAgreementStatus.Created), "move");
    }

    public static void RunAll()
    {
        LenientSuccessionPlanAgreementStartsInCreated();
        LenientSuccessionPlanAgreementMovesToValidated();
        LenientSuccessionPlanAgreementRejectsMovesFromRetired();
    }
}
