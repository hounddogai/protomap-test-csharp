namespace Acme.Workforce.Assets.Tests.Warranty.Adjustment;

using Acme.Workforce.Assets.Warranty.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class WarrantyAdjustmentChecks
{
    public static void WarrantyAdjustmentStartsInRequested()
    {
        var item = WarrantyAdjustment.NewWarrantyAdjustment("code-850", "title-524");
        Check.Equal(item.Status, WarrantyAdjustmentStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantyAdjustmentMovesToExpired()
    {
        var item = WarrantyAdjustment.NewWarrantyAdjustment("code-850", "title-524");
        try
        {
            item.MoveTo(WarrantyAdjustmentStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantyAdjustmentStatus.Expired, "status");
    }

    public static void WarrantyAdjustmentRejectsMovesFromExpired()
    {
        var item = WarrantyAdjustment.NewWarrantyAdjustment("code-850", "title-524");
        item.Status = WarrantyAdjustmentStatus.Expired;
        Check.Fails(() => item.MoveTo(WarrantyAdjustmentStatus.Requested), "move");
    }

    public static void RunAll()
    {
        WarrantyAdjustmentStartsInRequested();
        WarrantyAdjustmentMovesToExpired();
        WarrantyAdjustmentRejectsMovesFromExpired();
    }
}
