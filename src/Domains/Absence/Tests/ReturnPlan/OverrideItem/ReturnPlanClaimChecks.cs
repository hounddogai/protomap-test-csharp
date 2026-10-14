namespace Acme.Workforce.Absence.Tests.ReturnPlan.OverrideItem;

using Acme.Workforce.Absence.ReturnPlan.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class ReturnPlanClaimChecks
{
    public static void ReturnPlanClaimStartsInOpen()
    {
        var item = ReturnPlanClaim.NewReturnPlanClaim("code-65", "title-81");
        Check.Equal(item.Status, ReturnPlanClaimStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReturnPlanClaimMovesToResolved()
    {
        var item = ReturnPlanClaim.NewReturnPlanClaim("code-65", "title-81");
        try
        {
            item.MoveTo(ReturnPlanClaimStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReturnPlanClaimStatus.Resolved, "status");
    }

    public static void ReturnPlanClaimRejectsMovesFromArchived()
    {
        var item = ReturnPlanClaim.NewReturnPlanClaim("code-65", "title-81");
        item.Status = ReturnPlanClaimStatus.Archived;
        Check.Fails(() => item.MoveTo(ReturnPlanClaimStatus.Open), "move");
    }

    public static void RunAll()
    {
        ReturnPlanClaimStartsInOpen();
        ReturnPlanClaimMovesToResolved();
        ReturnPlanClaimRejectsMovesFromArchived();
    }
}
