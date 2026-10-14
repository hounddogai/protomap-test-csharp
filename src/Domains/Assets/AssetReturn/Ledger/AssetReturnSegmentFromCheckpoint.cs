namespace Acme.Workforce.Assets.AssetReturn.Ledger;

using Acme.Workforce.Assets.Laptop.Threshold;

/// <summary>Functions of asset return segment from checkpoint.</summary>
public static class AssetReturnSegmentFromCheckpoint
{
    public static Dictionary<string, double> SummarizeAdaptiveLaptopCheckpointsForSegment(List<AdaptiveLaptopCheckpoint> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<AssetReturnSegment> DeriveAssetReturnSegmentsFromCheckpoint(List<AdaptiveLaptopCheckpoint> items)
    {
        var outValue = new List<AssetReturnSegment>();
        foreach (var item in items)
        {
            var derived = AssetReturnSegment.NewAssetReturnSegment(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeAssetReturnSegment(AssetReturnSegment item)
    {
        return AdaptiveLaptopCheckpointPolicy.ClassifyAdaptiveLaptopCheckpoint(item.EffectiveExposure());
    }
}
