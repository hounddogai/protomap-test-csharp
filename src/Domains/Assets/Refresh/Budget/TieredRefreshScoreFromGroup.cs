namespace Acme.Workforce.Assets.Refresh.Budget;

using Acme.Workforce.Assets.Laptop.Metric;

/// <summary>Functions of tiered refresh score from group.</summary>
public static class TieredRefreshScoreFromGroup
{
    public static Dictionary<string, double> SummarizeLaptopGroupsForScore(List<LaptopGroup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<TieredRefreshScore> DeriveTieredRefreshScoresFromGroup(List<LaptopGroup> items)
    {
        var outValue = new List<TieredRefreshScore>();
        foreach (var item in items)
        {
            var derived = TieredRefreshScore.NewTieredRefreshScore(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyTieredRefreshScore(TieredRefreshScore item)
    {
        return LaptopGroupPolicy.LaptopGroupBucket(item.WeightedValue());
    }
}
