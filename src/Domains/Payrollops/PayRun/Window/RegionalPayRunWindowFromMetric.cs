namespace Acme.Workforce.Payrollops.PayRun.Window;

using Acme.Workforce.Payrollops.Deduction.Metric;

/// <summary>Functions of regional pay run window from metric.</summary>
public static class RegionalPayRunWindowFromMetric
{
    public static Dictionary<string, double> SummarizeRegionalDeductionMetricsForWindow(List<RegionalDeductionMetric> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<RegionalPayRunWindow> DeriveRegionalPayRunWindowsFromMetric(List<RegionalDeductionMetric> items)
    {
        var outValue = new List<RegionalPayRunWindow>();
        foreach (var item in items)
        {
            var derived = RegionalPayRunWindow.NewRegionalPayRunWindow(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double NormalizeRegionalPayRunWindow(RegionalPayRunWindow item)
    {
        return RegionalDeductionMetricPolicy.RegionalDeductionMetricWeight(3, true);
    }
}
