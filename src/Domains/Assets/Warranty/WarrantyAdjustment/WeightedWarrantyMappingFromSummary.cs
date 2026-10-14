namespace Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

using Acme.Workforce.Assets.Refresh.Plan;

/// <summary>Functions of weighted warranty mapping from summary.</summary>
public static class WeightedWarrantyMappingFromSummary
{
    public static Dictionary<string, double> SummarizeLegacyRefreshSummariesForMapping(List<LegacyRefreshSummary> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<WeightedWarrantyMapping> DeriveWeightedWarrantyMappingsFromSummary(List<LegacyRefreshSummary> items)
    {
        var outValue = new List<WeightedWarrantyMapping>();
        foreach (var item in items)
        {
            var derived = WeightedWarrantyMapping.NewWeightedWarrantyMapping(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectWeightedWarrantyMapping(WeightedWarrantyMapping item)
    {
        return RefreshPlanPolicy.ClassifyRefreshPlan(item.EffectiveValue());
    }
}
