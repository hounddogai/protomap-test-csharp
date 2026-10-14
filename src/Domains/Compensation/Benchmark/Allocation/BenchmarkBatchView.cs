namespace Acme.Workforce.Compensation.Benchmark.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a benchmark batch.</summary>
public sealed record BenchmarkBatchView(string CodeText, string LabelText, string CreatedAt, string WindowDays, string ExpiresAt, string StatusLabel)
{
    public static BenchmarkBatchView ToBenchmarkBatchView(BenchmarkBatch item)
    {
        return new BenchmarkBatchView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BenchmarkBatchView> ToBenchmarkBatchViews(List<BenchmarkBatch> items)
    {
        var views = new List<BenchmarkBatchView>();
        foreach (var item in items)
        {
            views.Add(ToBenchmarkBatchView(item));
        }
        return views;
    }

    public static double SumBenchmarkBatchView(BenchmarkBatchView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedWindowDays + (double)parsedExpiresAt;
    }
}
