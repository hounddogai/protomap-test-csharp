namespace Acme.Workforce.Assets.AssetReturn.Agreement;

using Acme.Workforce.Assets.Warranty.Assignment;

/// <summary>Functions of asset return group from assignment.</summary>
public static class AssetReturnGroupFromAssignment
{
    public static Dictionary<string, double> SummarizeWarrantyAssignmentsForGroup(List<WarrantyAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<AssetReturnGroup> DeriveAssetReturnGroupsFromAssignment(List<WarrantyAssignment> items)
    {
        var outValue = new List<AssetReturnGroup>();
        foreach (var item in items)
        {
            var derived = AssetReturnGroup.NewAssetReturnGroup(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeAssetReturnGroup(AssetReturnGroup item)
    {
        return WarrantyProfilePolicy.WarrantyProfileBucket(item.NetTotal());
    }
}
