namespace Acme.Workforce.Absence.Tests.ReturnPlan.Agreement;

using Acme.Workforce.Absence.ReturnPlan.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class ReturnPlanAgreementChecks
{
    public static void ReturnPlanAgreementStartsInCreated()
    {
        var item = ReturnPlanAgreement.NewReturnPlanAgreement("code-255", "label-682");
        Check.Equal(item.Status, ReturnPlanAgreementStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReturnPlanAgreementMovesToRetired()
    {
        var item = ReturnPlanAgreement.NewReturnPlanAgreement("code-255", "label-682");
        try
        {
            item.MoveTo(ReturnPlanAgreementStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReturnPlanAgreementStatus.Retired, "status");
    }

    public static void ReturnPlanAgreementRejectsMovesFromRetired()
    {
        var item = ReturnPlanAgreement.NewReturnPlanAgreement("code-255", "label-682");
        item.Status = ReturnPlanAgreementStatus.Retired;
        Check.Fails(() => item.MoveTo(ReturnPlanAgreementStatus.Created), "move");
    }

    public static void RunAll()
    {
        ReturnPlanAgreementStartsInCreated();
        ReturnPlanAgreementMovesToRetired();
        ReturnPlanAgreementRejectsMovesFromRetired();
    }
}
