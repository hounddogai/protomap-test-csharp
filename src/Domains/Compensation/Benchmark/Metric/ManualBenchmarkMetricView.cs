namespace Acme.Workforce.Compensation.Benchmark.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual benchmark metric.</summary>
public sealed record ManualBenchmarkMetricView(string Code, string LabelText, string Attempts, string Active, string StartsAt, string CreatedAt, string QuantityText, string StatusLabel)
{
    public static ManualBenchmarkMetricView ToManualBenchmarkMetricView(ManualBenchmarkMetric item)
    {
        return new ManualBenchmarkMetricView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualBenchmarkMetricView> ToManualBenchmarkMetricViews(List<ManualBenchmarkMetric> items)
    {
        var views = new List<ManualBenchmarkMetricView>();
        foreach (var item in items)
        {
            views.Add(ToManualBenchmarkMetricView(item));
        }
        return views;
    }

    public static double SumManualBenchmarkMetricView(ManualBenchmarkMetricView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedStartsAt + (double)parsedCreatedAt;
    }
}
