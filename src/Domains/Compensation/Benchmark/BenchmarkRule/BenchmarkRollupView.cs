namespace Acme.Workforce.Compensation.Benchmark.BenchmarkRule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a benchmark rollup.</summary>
public sealed record BenchmarkRollupView(string Code, string NameText, string Country, string Timezone, string CeilingText, string Segment, string Retries, string OwnerId, string EndsAtText, string RevisionText, string StatusLabel)
{
    public static BenchmarkRollupView ToBenchmarkRollupView(BenchmarkRollup item)
    {
        return new BenchmarkRollupView(
            item.Code,
            item.Name,
            Text.Truncate(item.Country, 80),
            Text.Truncate(item.Timezone, 64),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Segment,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.OwnerId, 40),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BenchmarkRollupView> ToBenchmarkRollupViews(List<BenchmarkRollup> items)
    {
        var views = new List<BenchmarkRollupView>();
        foreach (var item in items)
        {
            views.Add(ToBenchmarkRollupView(item));
        }
        return views;
    }

    public static double SumBenchmarkRollupView(BenchmarkRollupView view)
    {
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedRetries + (double)parsedEndsAt;
    }
}
