namespace Acme.Workforce.Compensation.Benchmark.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default benchmark rollup.</summary>
public sealed record DefaultBenchmarkRollupView(string CodeText, string Label, string Segment, string Grade, string Capacity, string Reviewer, string StatusLabel)
{
    public static DefaultBenchmarkRollupView ToDefaultBenchmarkRollupView(DefaultBenchmarkRollup item)
    {
        return new DefaultBenchmarkRollupView(
            item.Code,
            Text.Truncate(item.Label, 64),
            Text.Truncate(item.Segment, 64),
            Text.Truncate(item.Grade, 120),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultBenchmarkRollupView> ToDefaultBenchmarkRollupViews(List<DefaultBenchmarkRollup> items)
    {
        var views = new List<DefaultBenchmarkRollupView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultBenchmarkRollupView(item));
        }
        return views;
    }

    public static double SumDefaultBenchmarkRollupView(DefaultBenchmarkRollupView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return (double)parsedCapacity;
    }
}
