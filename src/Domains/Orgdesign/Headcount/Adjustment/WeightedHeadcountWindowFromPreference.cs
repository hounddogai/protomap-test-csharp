namespace Acme.Workforce.Orgdesign.Headcount.Adjustment;

using Acme.Workforce.Orgdesign.CostModel.Preference;

/// <summary>Functions of weighted headcount window from preference.</summary>
public static class WeightedHeadcountWindowFromPreference
{
    public static Dictionary<string, double> SummarizeSeasonalCostModelPreferencesForWindow(List<SeasonalCostModelPreference> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<WeightedHeadcountWindow> DeriveWeightedHeadcountWindowsFromPreference(List<SeasonalCostModelPreference> items)
    {
        var outValue = new List<WeightedHeadcountWindow>();
        foreach (var item in items)
        {
            var derived = WeightedHeadcountWindow.NewWeightedHeadcountWindow(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateWeightedHeadcountWindow(WeightedHeadcountWindow item)
    {
        return CostModelGroupPolicy.CostModelGroupBucket(item.NetValue());
    }
}
