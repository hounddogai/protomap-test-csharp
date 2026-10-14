namespace Acme.Workforce.Contractors.Tests.Vendor.Agreement;

using Acme.Workforce.Contractors.Vendor.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class BatchedVendorOverrideChecks
{
    public static void BatchedVendorOverrideStartsInProposed()
    {
        var item = BatchedVendorOverride.NewBatchedVendorOverride("code-60", "name-147");
        Check.Equal(item.Status, BatchedVendorOverrideStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedVendorOverrideMovesToApproved()
    {
        var item = BatchedVendorOverride.NewBatchedVendorOverride("code-60", "name-147");
        try
        {
            item.MoveTo(BatchedVendorOverrideStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedVendorOverrideStatus.Approved, "status");
    }

    public static void BatchedVendorOverrideRejectsMovesFromWithdrawn()
    {
        var item = BatchedVendorOverride.NewBatchedVendorOverride("code-60", "name-147");
        item.Status = BatchedVendorOverrideStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(BatchedVendorOverrideStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        BatchedVendorOverrideStartsInProposed();
        BatchedVendorOverrideMovesToApproved();
        BatchedVendorOverrideRejectsMovesFromWithdrawn();
    }
}
