namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyAllocation;

using Acme.Workforce.Assets.Warranty.WarrantyAllocation;

/// <summary>Checks of warranty allocation code, run by the project's runner.</summary>
public static class PendingWarrantyAllocationChecks
{
    public static void PendingWarrantyAllocationStartsInProposed()
    {
        var item = PendingWarrantyAllocation.NewPendingWarrantyAllocation("code-596", "title-281");
        Check.Equal(item.Status, PendingWarrantyAllocationStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingWarrantyAllocationMovesToRejected()
    {
        var item = PendingWarrantyAllocation.NewPendingWarrantyAllocation("code-596", "title-281");
        try
        {
            item.MoveTo(PendingWarrantyAllocationStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingWarrantyAllocationStatus.Rejected, "status");
    }

    public static void PendingWarrantyAllocationRejectsMovesFromWithdrawn()
    {
        var item = PendingWarrantyAllocation.NewPendingWarrantyAllocation("code-596", "title-281");
        item.Status = PendingWarrantyAllocationStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PendingWarrantyAllocationStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PendingWarrantyAllocationStartsInProposed();
        PendingWarrantyAllocationMovesToRejected();
        PendingWarrantyAllocationRejectsMovesFromWithdrawn();
    }
}
