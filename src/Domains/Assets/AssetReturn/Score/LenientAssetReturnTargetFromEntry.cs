namespace Acme.Workforce.Assets.AssetReturn.Score;

using Acme.Workforce.Assets.AssetReturn.ReturnReservation;

/// <summary>Functions of lenient asset return target from entry.</summary>
public static class LenientAssetReturnTargetFromEntry
{
    public static Dictionary<string, double> SummarizeAssetReturnEntriesForTarget(List<AssetReturnEntry> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<LenientAssetReturnTarget> DeriveLenientAssetReturnTargetsFromEntry(List<AssetReturnEntry> items)
    {
        var outValue = new List<LenientAssetReturnTarget>();
        foreach (var item in items)
        {
            var derived = LenientAssetReturnTarget.NewLenientAssetReturnTarget(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreLenientAssetReturnTarget(LenientAssetReturnTarget item)
    {
        return AssetReturnEntryPolicy.ClassifyAssetReturnEntry(item.MeasuredValue());
    }
}
