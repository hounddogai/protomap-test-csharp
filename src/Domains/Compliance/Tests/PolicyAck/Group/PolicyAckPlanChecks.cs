namespace Acme.Workforce.Compliance.Tests.PolicyAck.Group;

using Acme.Workforce.Compliance.PolicyAck.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PolicyAckPlanChecks
{
    public static void PolicyAckPlanStartsInIdle()
    {
        var item = PolicyAckPlan.NewPolicyAckPlan("code-66", "title-970");
        Check.Equal(item.Status, PolicyAckPlanStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyAckPlanMovesToWarming()
    {
        var item = PolicyAckPlan.NewPolicyAckPlan("code-66", "title-970");
        try
        {
            item.MoveTo(PolicyAckPlanStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyAckPlanStatus.Warming, "status");
    }

    public static void PolicyAckPlanRejectsMovesFromStopped()
    {
        var item = PolicyAckPlan.NewPolicyAckPlan("code-66", "title-970");
        item.Status = PolicyAckPlanStatus.Stopped;
        Check.Fails(() => item.MoveTo(PolicyAckPlanStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PolicyAckPlanStartsInIdle();
        PolicyAckPlanMovesToWarming();
        PolicyAckPlanRejectsMovesFromStopped();
    }
}
