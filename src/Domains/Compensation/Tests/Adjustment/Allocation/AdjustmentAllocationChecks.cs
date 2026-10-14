namespace Acme.Workforce.Compensation.Tests.Adjustment.Allocation;

using Acme.Workforce.Compensation.Adjustment.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class AdjustmentAllocationChecks
{
    public static void AdjustmentAllocationStartsInProposed()
    {
        var item = AdjustmentAllocation.NewAdjustmentAllocation("code-181", "name-764");
        Check.Equal(item.Status, AdjustmentAllocationStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdjustmentAllocationMovesToApproved()
    {
        var item = AdjustmentAllocation.NewAdjustmentAllocation("code-181", "name-764");
        try
        {
            item.MoveTo(AdjustmentAllocationStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdjustmentAllocationStatus.Approved, "status");
    }

    public static void AdjustmentAllocationRejectsMovesFromWithdrawn()
    {
        var item = AdjustmentAllocation.NewAdjustmentAllocation("code-181", "name-764");
        item.Status = AdjustmentAllocationStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AdjustmentAllocationStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AdjustmentAllocationStartsInProposed();
        AdjustmentAllocationMovesToApproved();
        AdjustmentAllocationRejectsMovesFromWithdrawn();
    }
}
