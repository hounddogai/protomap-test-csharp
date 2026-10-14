namespace Acme.Workforce.Assets.Laptop.LaptopAdjustment;

using Acme.Workforce.Assets.Laptop.Target;

/// <summary>Functions of default laptop digest from estimate.</summary>
public static class DefaultLaptopDigestFromEstimate
{
    public static Dictionary<string, double> SummarizeWeightedLaptopEstimatesForDigest(List<WeightedLaptopEstimate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<DefaultLaptopDigest> DeriveDefaultLaptopDigestsFromEstimate(List<WeightedLaptopEstimate> items)
    {
        var outValue = new List<DefaultLaptopDigest>();
        foreach (var item in items)
        {
            var derived = DefaultLaptopDigest.NewDefaultLaptopDigest(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateDefaultLaptopDigest(DefaultLaptopDigest item)
    {
        return PendingLaptopTargetPolicy.PendingLaptopTargetBucket(item.EffectiveExposure());
    }
}
