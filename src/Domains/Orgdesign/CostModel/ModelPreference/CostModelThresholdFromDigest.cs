namespace Acme.Workforce.Orgdesign.CostModel.ModelPreference;

using Acme.Workforce.Orgdesign.Position.Profile;

/// <summary>Functions of cost model threshold from digest.</summary>
public static class CostModelThresholdFromDigest
{
    public static Dictionary<string, double> SummarizePositionDigestsForThreshold(List<PositionDigest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<CostModelThreshold> DeriveCostModelThresholdsFromDigest(List<PositionDigest> items)
    {
        var outValue = new List<CostModelThreshold>();
        foreach (var item in items)
        {
            var derived = CostModelThreshold.NewCostModelThreshold(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignCostModelThreshold(CostModelThreshold item)
    {
        return GlobalPositionRollupPolicy.GlobalPositionRollupBucket(item.ProjectedTotal());
    }
}
