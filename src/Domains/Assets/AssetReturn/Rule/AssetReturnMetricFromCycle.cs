namespace Acme.Workforce.Assets.AssetReturn.Rule;

using Acme.Workforce.Assets.LicenseSeat.Cycle;

/// <summary>Functions of asset return metric from cycle.</summary>
public static class AssetReturnMetricFromCycle
{
    public static Dictionary<string, double> SummarizeCappedLicenseSeatCyclesForMetric(List<CappedLicenseSeatCycle> items)
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

    public static List<AssetReturnMetric> DeriveAssetReturnMetricsFromCycle(List<CappedLicenseSeatCycle> items)
    {
        var outValue = new List<AssetReturnMetric>();
        foreach (var item in items)
        {
            var derived = AssetReturnMetric.NewAssetReturnMetric(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double EstimateAssetReturnMetric(AssetReturnMetric item)
    {
        return CappedLicenseSeatCyclePolicy.CappedLicenseSeatCycleWeight(5, true);
    }
}
