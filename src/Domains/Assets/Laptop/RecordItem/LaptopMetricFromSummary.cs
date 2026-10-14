namespace Acme.Workforce.Assets.Laptop.RecordItem;

using Acme.Workforce.Assets.LicenseSeat.Summary;

/// <summary>Functions of laptop metric from summary.</summary>
public static class LaptopMetricFromSummary
{
    public static Dictionary<string, double> SummarizeDefaultLicenseSeatSummariesForMetric(List<DefaultLicenseSeatSummary> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<LaptopMetric> DeriveLaptopMetricsFromSummary(List<DefaultLicenseSeatSummary> items)
    {
        var outValue = new List<LaptopMetric>();
        foreach (var item in items)
        {
            var derived = LaptopMetric.NewLaptopMetric(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignLaptopMetric(LaptopMetric item)
    {
        return LicenseSeatBudgetPolicy.LicenseSeatBudgetBucket(item.EffectiveLoad());
    }
}
