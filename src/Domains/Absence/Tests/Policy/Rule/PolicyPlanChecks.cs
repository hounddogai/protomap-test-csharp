namespace Acme.Workforce.Absence.Tests.Policy.Rule;

using Acme.Workforce.Absence.Policy.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PolicyPlanChecks
{
    public static void PolicyPlanStartsInNew()
    {
        var item = PolicyPlan.NewPolicyPlan("code-514", "label-866");
        Check.Equal(item.Status, PolicyPlanStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyPlanMovesToQueued()
    {
        var item = PolicyPlan.NewPolicyPlan("code-514", "label-866");
        try
        {
            item.MoveTo(PolicyPlanStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyPlanStatus.Queued, "status");
    }

    public static void PolicyPlanRejectsMovesFromFailed()
    {
        var item = PolicyPlan.NewPolicyPlan("code-514", "label-866");
        item.Status = PolicyPlanStatus.Failed;
        Check.Fails(() => item.MoveTo(PolicyPlanStatus.New), "move");
    }

    public static void RunAll()
    {
        PolicyPlanStartsInNew();
        PolicyPlanMovesToQueued();
        PolicyPlanRejectsMovesFromFailed();
    }
}
