namespace Acme.Workforce.Absence.Tests.ReturnPlan.Bundle;

using Acme.Workforce.Absence.ReturnPlan.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class ReturnPlanBundleChecks
{
    public static void ReturnPlanBundleStartsInRequested()
    {
        var item = ReturnPlanBundle.NewReturnPlanBundle("code-189", "name-542");
        Check.Equal(item.Status, ReturnPlanBundleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReturnPlanBundleMovesToRevoked()
    {
        var item = ReturnPlanBundle.NewReturnPlanBundle("code-189", "name-542");
        try
        {
            item.MoveTo(ReturnPlanBundleStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReturnPlanBundleStatus.Revoked, "status");
    }

    public static void ReturnPlanBundleRejectsMovesFromExpired()
    {
        var item = ReturnPlanBundle.NewReturnPlanBundle("code-189", "name-542");
        item.Status = ReturnPlanBundleStatus.Expired;
        Check.Fails(() => item.MoveTo(ReturnPlanBundleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ReturnPlanBundleStartsInRequested();
        ReturnPlanBundleMovesToRevoked();
        ReturnPlanBundleRejectsMovesFromExpired();
    }
}
