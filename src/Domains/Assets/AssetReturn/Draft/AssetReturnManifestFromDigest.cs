namespace Acme.Workforce.Assets.AssetReturn.Draft;

using Acme.Workforce.Assets.Assignment.Allocation;

/// <summary>Functions of asset return manifest from digest.</summary>
public static class AssetReturnManifestFromDigest
{
    public static Dictionary<string, double> SummarizeAssignmentDigestsForManifest(List<AssignmentDigest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<AssetReturnManifest> DeriveAssetReturnManifestsFromDigest(List<AssignmentDigest> items)
    {
        var outValue = new List<AssetReturnManifest>();
        foreach (var item in items)
        {
            var derived = AssetReturnManifest.NewAssetReturnManifest(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AllocateAssetReturnManifest(AssetReturnManifest item)
    {
        return AssignmentThresholdPolicy.AssignmentThresholdWeight(4, true);
    }
}
