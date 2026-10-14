namespace Acme.Workforce.Contractors.Tests.Offboarding.CappedOffboardingSlot;

using Acme.Workforce.Contractors.Offboarding.CappedOffboardingSlot;

/// <summary>Checks of capped offboarding slot code, run by the project's runner.</summary>
public static class BatchedOffboardingAllocationChecks
{
    public static void BatchedOffboardingAllocationStartsInPlanned()
    {
        var item = BatchedOffboardingAllocation.NewBatchedOffboardingAllocation("code-632", "label-224");
        Check.Equal(item.Status, BatchedOffboardingAllocationStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedOffboardingAllocationMovesToCancelled()
    {
        var item = BatchedOffboardingAllocation.NewBatchedOffboardingAllocation("code-632", "label-224");
        try
        {
            item.MoveTo(BatchedOffboardingAllocationStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedOffboardingAllocationStatus.Cancelled, "status");
    }

    public static void BatchedOffboardingAllocationRejectsMovesFromCancelled()
    {
        var item = BatchedOffboardingAllocation.NewBatchedOffboardingAllocation("code-632", "label-224");
        item.Status = BatchedOffboardingAllocationStatus.Cancelled;
        Check.Fails(() => item.MoveTo(BatchedOffboardingAllocationStatus.Planned), "move");
    }

    public static void RunAll()
    {
        BatchedOffboardingAllocationStartsInPlanned();
        BatchedOffboardingAllocationMovesToCancelled();
        BatchedOffboardingAllocationRejectsMovesFromCancelled();
    }
}
