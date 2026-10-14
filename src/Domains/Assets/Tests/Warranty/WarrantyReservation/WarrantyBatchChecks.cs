namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyReservation;

using Acme.Workforce.Assets.Warranty.WarrantyReservation;

/// <summary>Checks of warranty reservation code, run by the project's runner.</summary>
public static class WarrantyBatchChecks
{
    public static void WarrantyBatchStartsInProposed()
    {
        var item = WarrantyBatch.NewWarrantyBatch("code-422", "name-840");
        Check.Equal(item.Status, WarrantyBatchStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantyBatchMovesToApproved()
    {
        var item = WarrantyBatch.NewWarrantyBatch("code-422", "name-840");
        try
        {
            item.MoveTo(WarrantyBatchStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantyBatchStatus.Approved, "status");
    }

    public static void WarrantyBatchRejectsMovesFromWithdrawn()
    {
        var item = WarrantyBatch.NewWarrantyBatch("code-422", "name-840");
        item.Status = WarrantyBatchStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(WarrantyBatchStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        WarrantyBatchStartsInProposed();
        WarrantyBatchMovesToApproved();
        WarrantyBatchRejectsMovesFromWithdrawn();
    }
}
