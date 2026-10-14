namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyAdjustment;

using Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

/// <summary>Checks of warranty adjustment code, run by the project's runner.</summary>
public static class LenientWarrantyAdjustmentChecks
{
    public static void LenientWarrantyAdjustmentStartsInCreated()
    {
        var item = LenientWarrantyAdjustment.NewLenientWarrantyAdjustment("code-106", "title-48");
        Check.Equal(item.Status, LenientWarrantyAdjustmentStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientWarrantyAdjustmentMovesToValidated()
    {
        var item = LenientWarrantyAdjustment.NewLenientWarrantyAdjustment("code-106", "title-48");
        try
        {
            item.MoveTo(LenientWarrantyAdjustmentStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientWarrantyAdjustmentStatus.Validated, "status");
    }

    public static void LenientWarrantyAdjustmentRejectsMovesFromRetired()
    {
        var item = LenientWarrantyAdjustment.NewLenientWarrantyAdjustment("code-106", "title-48");
        item.Status = LenientWarrantyAdjustmentStatus.Retired;
        Check.Fails(() => item.MoveTo(LenientWarrantyAdjustmentStatus.Created), "move");
    }

    public static void RunAll()
    {
        LenientWarrantyAdjustmentStartsInCreated();
        LenientWarrantyAdjustmentMovesToValidated();
        LenientWarrantyAdjustmentRejectsMovesFromRetired();
    }
}
