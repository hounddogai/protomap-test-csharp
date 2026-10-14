namespace Acme.Workforce.Assets.Laptop.RecordItem;

using Acme.Workforce.Assets.Laptop.Mapping;

/// <summary>Functions of laptop metric from window.</summary>
public static class LaptopMetricFromWindow
{
    public static Dictionary<string, double> SummarizeDefaultLaptopWindowsForMetric(List<DefaultLaptopWindow> items)
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

    public static List<LaptopMetric> DeriveLaptopMetricsFromWindow(List<DefaultLaptopWindow> items)
    {
        var outValue = new List<LaptopMetric>();
        foreach (var item in items)
        {
            var derived = LaptopMetric.NewLaptopMetric(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeLaptopMetric(LaptopMetric item)
    {
        return DefaultLaptopWindowPolicy.NormalizeDefaultLaptopWindowCode(item.Code);
    }
}
