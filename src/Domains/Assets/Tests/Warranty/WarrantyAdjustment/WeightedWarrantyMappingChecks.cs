namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyAdjustment;

using Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

/// <summary>Checks of warranty adjustment code, run by the project's runner.</summary>
public static class WeightedWarrantyMappingChecks
{
    public static void WeightedWarrantyMappingStartsInProposed()
    {
        var item = WeightedWarrantyMapping.NewWeightedWarrantyMapping("code-496", "label-839");
        Check.Equal(item.Status, WeightedWarrantyMappingStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedWarrantyMappingMovesToRejected()
    {
        var item = WeightedWarrantyMapping.NewWeightedWarrantyMapping("code-496", "label-839");
        try
        {
            item.MoveTo(WeightedWarrantyMappingStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedWarrantyMappingStatus.Rejected, "status");
    }

    public static void WeightedWarrantyMappingRejectsMovesFromWithdrawn()
    {
        var item = WeightedWarrantyMapping.NewWeightedWarrantyMapping("code-496", "label-839");
        item.Status = WeightedWarrantyMappingStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(WeightedWarrantyMappingStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        WeightedWarrantyMappingStartsInProposed();
        WeightedWarrantyMappingMovesToRejected();
        WeightedWarrantyMappingRejectsMovesFromWithdrawn();
    }
}
