namespace Acme.Workforce.Compensation.Benchmark.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a benchmark claim.</summary>
public sealed record BenchmarkClaimView(string Code, string Name, string Priority, string CostCenter, string Archived, string Amount, string Timezone, string BatchSize, string Cost, string Locale, string StatusLabel)
{
    public static BenchmarkClaimView ToBenchmarkClaimView(BenchmarkClaim item)
    {
        return new BenchmarkClaimView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.CostCenter,
            $"{item.Archived}",
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Timezone, 64),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BenchmarkClaimView> ToBenchmarkClaimViews(List<BenchmarkClaim> items)
    {
        var views = new List<BenchmarkClaimView>();
        foreach (var item in items)
        {
            views.Add(ToBenchmarkClaimView(item));
        }
        return views;
    }

    public static double SumBenchmarkClaimView(BenchmarkClaimView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedPriority + parsedAmount + (double)parsedBatchSize;
    }
}
