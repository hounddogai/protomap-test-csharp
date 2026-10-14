namespace Acme.Workforce.Compensation.Benchmark.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a benchmark adjustment.</summary>
public sealed record BenchmarkAdjustmentView(string Code, string Label, string ExternalRefText, string BatchSize, string Skill, string StatusLabel)
{
    public static BenchmarkAdjustmentView ToBenchmarkAdjustmentView(BenchmarkAdjustment item)
    {
        return new BenchmarkAdjustmentView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.ExternalRef,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Skill, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BenchmarkAdjustmentView> ToBenchmarkAdjustmentViews(List<BenchmarkAdjustment> items)
    {
        var views = new List<BenchmarkAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToBenchmarkAdjustmentView(item));
        }
        return views;
    }

    public static double SumBenchmarkAdjustmentView(BenchmarkAdjustmentView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize;
    }
}
