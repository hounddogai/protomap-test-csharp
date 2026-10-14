namespace Acme.Workforce.Compensation.Benchmark.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a benchmark policy.</summary>
public sealed record BenchmarkPolicyView(string CodeText, string NameText, string Score, string Retries, string BatchSize, string StatusLabel)
{
    public static BenchmarkPolicyView ToBenchmarkPolicyView(BenchmarkPolicy item)
    {
        return new BenchmarkPolicyView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BenchmarkPolicyView> ToBenchmarkPolicyViews(List<BenchmarkPolicy> items)
    {
        var views = new List<BenchmarkPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToBenchmarkPolicyView(item));
        }
        return views;
    }

    public static double SumBenchmarkPolicyView(BenchmarkPolicyView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedRetries + (double)parsedBatchSize;
    }
}
