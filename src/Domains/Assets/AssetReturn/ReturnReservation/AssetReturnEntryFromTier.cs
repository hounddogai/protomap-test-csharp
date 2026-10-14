namespace Acme.Workforce.Assets.AssetReturn.ReturnReservation;

using Acme.Workforce.Assets.Assignment.Batch;

/// <summary>Functions of asset return entry from tier.</summary>
public static class AssetReturnEntryFromTier
{
    public static Dictionary<string, double> SummarizePrimaryAssignmentTiersForEntry(List<PrimaryAssignmentTier> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<AssetReturnEntry> DeriveAssetReturnEntriesFromTier(List<PrimaryAssignmentTier> items)
    {
        var outValue = new List<AssetReturnEntry>();
        foreach (var item in items)
        {
            var derived = AssetReturnEntry.NewAssetReturnEntry(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double PlanAssetReturnEntry(AssetReturnEntry item)
    {
        return DefaultAssignmentBatchPolicy.DefaultAssignmentBatchWeight(4, true);
    }
}
