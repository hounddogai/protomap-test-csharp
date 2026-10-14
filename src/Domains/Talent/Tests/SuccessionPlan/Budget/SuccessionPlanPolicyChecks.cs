namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Budget;

using Acme.Workforce.Talent.SuccessionPlan.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class SuccessionPlanPolicyChecks
{
    public static void SuccessionPlanPolicyStartsInIdle()
    {
        var item = SuccessionPlanPolicy.NewSuccessionPlanPolicy("code-915", "label-375");
        Check.Equal(item.Status, SuccessionPlanPolicyStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SuccessionPlanPolicyMovesToWarming()
    {
        var item = SuccessionPlanPolicy.NewSuccessionPlanPolicy("code-915", "label-375");
        try
        {
            item.MoveTo(SuccessionPlanPolicyStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SuccessionPlanPolicyStatus.Warming, "status");
    }

    public static void SuccessionPlanPolicyRejectsMovesFromStopped()
    {
        var item = SuccessionPlanPolicy.NewSuccessionPlanPolicy("code-915", "label-375");
        item.Status = SuccessionPlanPolicyStatus.Stopped;
        Check.Fails(() => item.MoveTo(SuccessionPlanPolicyStatus.Idle), "move");
    }

    public static void RunAll()
    {
        SuccessionPlanPolicyStartsInIdle();
        SuccessionPlanPolicyMovesToWarming();
        SuccessionPlanPolicyRejectsMovesFromStopped();
    }
}
