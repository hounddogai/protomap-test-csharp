namespace Acme.Workforce.Assets.Tests.AssetReturn.Variance;

using Acme.Workforce.Assets.AssetReturn.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class WeightedAssetReturnQuotaChecks
{
    public static void WeightedAssetReturnQuotaStartsInOpen()
    {
        var item = WeightedAssetReturnQuota.NewWeightedAssetReturnQuota("code-747", "title-490");
        Check.Equal(item.Status, WeightedAssetReturnQuotaStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedAssetReturnQuotaMovesToInReview()
    {
        var item = WeightedAssetReturnQuota.NewWeightedAssetReturnQuota("code-747", "title-490");
        try
        {
            item.MoveTo(WeightedAssetReturnQuotaStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedAssetReturnQuotaStatus.InReview, "status");
    }

    public static void WeightedAssetReturnQuotaRejectsMovesFromArchived()
    {
        var item = WeightedAssetReturnQuota.NewWeightedAssetReturnQuota("code-747", "title-490");
        item.Status = WeightedAssetReturnQuotaStatus.Archived;
        Check.Fails(() => item.MoveTo(WeightedAssetReturnQuotaStatus.Open), "move");
    }

    public static void RunAll()
    {
        WeightedAssetReturnQuotaStartsInOpen();
        WeightedAssetReturnQuotaMovesToInReview();
        WeightedAssetReturnQuotaRejectsMovesFromArchived();
    }
}
