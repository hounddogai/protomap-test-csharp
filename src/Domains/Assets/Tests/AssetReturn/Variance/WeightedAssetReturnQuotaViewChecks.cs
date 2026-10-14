namespace Acme.Workforce.Assets.Tests.AssetReturn.Variance;

using Acme.Workforce.Assets.AssetReturn.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class WeightedAssetReturnQuotaViewChecks
{
    public static void WeightedAssetReturnQuotaViewShowsTheStatusInCapitals()
    {
        var item = WeightedAssetReturnQuota.NewWeightedAssetReturnQuota("code-747", "title-490");
        var view = WeightedAssetReturnQuotaView.ToWeightedAssetReturnQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedAssetReturnQuotaView.ToWeightedAssetReturnQuotaViews(new List<WeightedAssetReturnQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedAssetReturnQuotaViewShowsTheStatusInCapitals();
    }
}
