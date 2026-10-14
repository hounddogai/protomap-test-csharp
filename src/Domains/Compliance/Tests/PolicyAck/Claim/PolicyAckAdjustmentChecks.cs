namespace Acme.Workforce.Compliance.Tests.PolicyAck.Claim;

using Acme.Workforce.Compliance.PolicyAck.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class PolicyAckAdjustmentChecks
{
    public static void PolicyAckAdjustmentStartsInIdle()
    {
        var item = PolicyAckAdjustment.NewPolicyAckAdjustment("code-651", "title-440");
        Check.Equal(item.Status, PolicyAckAdjustmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyAckAdjustmentMovesToWarming()
    {
        var item = PolicyAckAdjustment.NewPolicyAckAdjustment("code-651", "title-440");
        try
        {
            item.MoveTo(PolicyAckAdjustmentStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyAckAdjustmentStatus.Warming, "status");
    }

    public static void PolicyAckAdjustmentRejectsMovesFromStopped()
    {
        var item = PolicyAckAdjustment.NewPolicyAckAdjustment("code-651", "title-440");
        item.Status = PolicyAckAdjustmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(PolicyAckAdjustmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PolicyAckAdjustmentStartsInIdle();
        PolicyAckAdjustmentMovesToWarming();
        PolicyAckAdjustmentRejectsMovesFromStopped();
    }
}
