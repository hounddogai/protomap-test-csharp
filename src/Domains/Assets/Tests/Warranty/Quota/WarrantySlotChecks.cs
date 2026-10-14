namespace Acme.Workforce.Assets.Tests.Warranty.Quota;

using Acme.Workforce.Assets.Warranty.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class WarrantySlotChecks
{
    public static void WarrantySlotStartsInProposed()
    {
        var item = WarrantySlot.NewWarrantySlot("code-33", "label-806");
        Check.Equal(item.Status, WarrantySlotStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantySlotMovesToApproved()
    {
        var item = WarrantySlot.NewWarrantySlot("code-33", "label-806");
        try
        {
            item.MoveTo(WarrantySlotStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantySlotStatus.Approved, "status");
    }

    public static void WarrantySlotRejectsMovesFromWithdrawn()
    {
        var item = WarrantySlot.NewWarrantySlot("code-33", "label-806");
        item.Status = WarrantySlotStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(WarrantySlotStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        WarrantySlotStartsInProposed();
        WarrantySlotMovesToApproved();
        WarrantySlotRejectsMovesFromWithdrawn();
    }
}
