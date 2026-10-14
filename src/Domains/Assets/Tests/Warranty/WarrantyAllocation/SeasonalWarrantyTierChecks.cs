namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyAllocation;

using Acme.Workforce.Assets.Warranty.WarrantyAllocation;

/// <summary>Checks of warranty allocation code, run by the project's runner.</summary>
public static class SeasonalWarrantyTierChecks
{
    public static void SeasonalWarrantyTierStartsInNew()
    {
        var item = SeasonalWarrantyTier.NewSeasonalWarrantyTier("code-492", "name-348");
        Check.Equal(item.Status, SeasonalWarrantyTierStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalWarrantyTierMovesToQueued()
    {
        var item = SeasonalWarrantyTier.NewSeasonalWarrantyTier("code-492", "name-348");
        try
        {
            item.MoveTo(SeasonalWarrantyTierStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalWarrantyTierStatus.Queued, "status");
    }

    public static void SeasonalWarrantyTierRejectsMovesFromFailed()
    {
        var item = SeasonalWarrantyTier.NewSeasonalWarrantyTier("code-492", "name-348");
        item.Status = SeasonalWarrantyTierStatus.Failed;
        Check.Fails(() => item.MoveTo(SeasonalWarrantyTierStatus.New), "move");
    }

    public static void RunAll()
    {
        SeasonalWarrantyTierStartsInNew();
        SeasonalWarrantyTierMovesToQueued();
        SeasonalWarrantyTierRejectsMovesFromFailed();
    }
}
