namespace Acme.Workforce.Compliance.Tests.PolicyAck.Entry;

using Acme.Workforce.Compliance.PolicyAck.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class PolicyAckAllocationChecks
{
    public static void PolicyAckAllocationStartsInProposed()
    {
        var item = PolicyAckAllocation.NewPolicyAckAllocation("code-974", "label-131");
        Check.Equal(item.Status, PolicyAckAllocationStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyAckAllocationMovesToApproved()
    {
        var item = PolicyAckAllocation.NewPolicyAckAllocation("code-974", "label-131");
        try
        {
            item.MoveTo(PolicyAckAllocationStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyAckAllocationStatus.Approved, "status");
    }

    public static void PolicyAckAllocationRejectsMovesFromWithdrawn()
    {
        var item = PolicyAckAllocation.NewPolicyAckAllocation("code-974", "label-131");
        item.Status = PolicyAckAllocationStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PolicyAckAllocationStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PolicyAckAllocationStartsInProposed();
        PolicyAckAllocationMovesToApproved();
        PolicyAckAllocationRejectsMovesFromWithdrawn();
    }
}
