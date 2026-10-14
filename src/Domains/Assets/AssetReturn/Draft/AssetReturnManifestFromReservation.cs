namespace Acme.Workforce.Assets.AssetReturn.Draft;

using Acme.Workforce.Assets.AssetReturn.Reservation;

/// <summary>Functions of asset return manifest from reservation.</summary>
public static class AssetReturnManifestFromReservation
{
    public static Dictionary<string, double> SummarizeAssetReturnReservationsForManifest(List<AssetReturnReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<AssetReturnManifest> DeriveAssetReturnManifestsFromReservation(List<AssetReturnReservation> items)
    {
        var outValue = new List<AssetReturnManifest>();
        foreach (var item in items)
        {
            var derived = AssetReturnManifest.NewAssetReturnManifest(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeAssetReturnManifest(AssetReturnManifest item)
    {
        return AssetReturnCyclePolicy.ClassifyAssetReturnCycle(item.MeasuredValue());
    }
}
