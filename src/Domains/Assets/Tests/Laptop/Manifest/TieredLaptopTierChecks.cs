namespace Acme.Workforce.Assets.Tests.Laptop.Manifest;

using Acme.Workforce.Assets.Laptop.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class TieredLaptopTierChecks
{
    public static void TieredLaptopTierStartsInOpen()
    {
        var item = TieredLaptopTier.NewTieredLaptopTier("code-562", "label-317");
        Check.Equal(item.Status, TieredLaptopTierStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredLaptopTierMovesToInReview()
    {
        var item = TieredLaptopTier.NewTieredLaptopTier("code-562", "label-317");
        try
        {
            item.MoveTo(TieredLaptopTierStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredLaptopTierStatus.InReview, "status");
    }

    public static void TieredLaptopTierRejectsMovesFromArchived()
    {
        var item = TieredLaptopTier.NewTieredLaptopTier("code-562", "label-317");
        item.Status = TieredLaptopTierStatus.Archived;
        Check.Fails(() => item.MoveTo(TieredLaptopTierStatus.Open), "move");
    }

    public static void RunAll()
    {
        TieredLaptopTierStartsInOpen();
        TieredLaptopTierMovesToInReview();
        TieredLaptopTierRejectsMovesFromArchived();
    }
}
