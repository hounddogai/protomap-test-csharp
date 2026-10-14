namespace Acme.Workforce.Compensation.EquityGrant.Assignment;

using Acme.Workforce.Compensation.EquityGrant.Metric;

/// <summary>Functions of manual equity grant window from metric.</summary>
public static class ManualEquityGrantWindowFromMetric
{
    public static Dictionary<string, double> SummarizeEquityGrantMetricsForWindow(List<EquityGrantMetric> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<ManualEquityGrantWindow> DeriveManualEquityGrantWindowsFromMetric(List<EquityGrantMetric> items)
    {
        var outValue = new List<ManualEquityGrantWindow>();
        foreach (var item in items)
        {
            var derived = ManualEquityGrantWindow.NewManualEquityGrantWindow(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignManualEquityGrantWindow(ManualEquityGrantWindow item)
    {
        return EquityGrantMetricPolicy.ClassifyEquityGrantMetric(item.AdjustedExposure());
    }
}
