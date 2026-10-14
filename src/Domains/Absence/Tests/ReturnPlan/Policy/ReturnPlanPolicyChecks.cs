namespace Acme.Workforce.Absence.Tests.ReturnPlan.Policy;

using Acme.Workforce.Absence.ReturnPlan.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class ReturnPlanPolicyChecks
{
    public static void ReturnPlanPolicyStartsInNew()
    {
        var item = ReturnPlanPolicy.NewReturnPlanPolicy("code-937", "label-344");
        Check.Equal(item.Status, ReturnPlanPolicyStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReturnPlanPolicyMovesToRunning()
    {
        var item = ReturnPlanPolicy.NewReturnPlanPolicy("code-937", "label-344");
        try
        {
            item.MoveTo(ReturnPlanPolicyStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReturnPlanPolicyStatus.Running, "status");
    }

    public static void ReturnPlanPolicyRejectsMovesFromFailed()
    {
        var item = ReturnPlanPolicy.NewReturnPlanPolicy("code-937", "label-344");
        item.Status = ReturnPlanPolicyStatus.Failed;
        Check.Fails(() => item.MoveTo(ReturnPlanPolicyStatus.New), "move");
    }

    public static void RunAll()
    {
        ReturnPlanPolicyStartsInNew();
        ReturnPlanPolicyMovesToRunning();
        ReturnPlanPolicyRejectsMovesFromFailed();
    }
}
