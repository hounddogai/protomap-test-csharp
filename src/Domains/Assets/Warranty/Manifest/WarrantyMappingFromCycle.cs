namespace Acme.Workforce.Assets.Warranty.Manifest;

using Acme.Workforce.Assets.Refresh.Metric;

/// <summary>Functions of warranty mapping from cycle.</summary>
public static class WarrantyMappingFromCycle
{
    public static Dictionary<string, double> SummarizeRefreshCyclesForMapping(List<RefreshCycle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<WarrantyMapping> DeriveWarrantyMappingsFromCycle(List<RefreshCycle> items)
    {
        var outValue = new List<WarrantyMapping>();
        foreach (var item in items)
        {
            var derived = WarrantyMapping.NewWarrantyMapping(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ClassifyWarrantyMappingExtra(WarrantyMapping item)
    {
        return RefreshMetricPolicy.RefreshMetricWeight(8, true);
    }
}
