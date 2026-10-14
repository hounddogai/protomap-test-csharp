namespace Acme.Workforce.Compensation.Benchmark.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a benchmark rule.</summary>
public sealed record BenchmarkRuleView(string CodeText, string Label, string Retries, string Threshold, string Approved, string StatusLabel)
{
    public static BenchmarkRuleView ToBenchmarkRuleView(BenchmarkRule item)
    {
        return new BenchmarkRuleView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Label, 120),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BenchmarkRuleView> ToBenchmarkRuleViews(List<BenchmarkRule> items)
    {
        var views = new List<BenchmarkRuleView>();
        foreach (var item in items)
        {
            views.Add(ToBenchmarkRuleView(item));
        }
        return views;
    }

    public static double SumBenchmarkRuleView(BenchmarkRuleView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedThreshold;
    }
}
