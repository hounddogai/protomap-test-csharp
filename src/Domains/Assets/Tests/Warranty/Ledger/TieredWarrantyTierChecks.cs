namespace Acme.Workforce.Assets.Tests.Warranty.Ledger;

using Acme.Workforce.Assets.Warranty.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class TieredWarrantyTierChecks
{
    public static void TieredWarrantyTierStartsInIdle()
    {
        var item = TieredWarrantyTier.NewTieredWarrantyTier("code-219", "label-342");
        Check.Equal(item.Status, TieredWarrantyTierStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredWarrantyTierMovesToWarming()
    {
        var item = TieredWarrantyTier.NewTieredWarrantyTier("code-219", "label-342");
        try
        {
            item.MoveTo(TieredWarrantyTierStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredWarrantyTierStatus.Warming, "status");
    }

    public static void TieredWarrantyTierRejectsMovesFromStopped()
    {
        var item = TieredWarrantyTier.NewTieredWarrantyTier("code-219", "label-342");
        item.Status = TieredWarrantyTierStatus.Stopped;
        Check.Fails(() => item.MoveTo(TieredWarrantyTierStatus.Idle), "move");
    }

    public static void RunAll()
    {
        TieredWarrantyTierStartsInIdle();
        TieredWarrantyTierMovesToWarming();
        TieredWarrantyTierRejectsMovesFromStopped();
    }
}
