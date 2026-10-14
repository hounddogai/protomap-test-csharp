namespace Acme.Workforce.Compensation.Benchmark.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a benchmark metric.</summary>
public sealed record BenchmarkMetricView(string Code, string Title, string NameText, string MaximumText, string CeilingText, string Rating, string Location, string WindowDays, string Phone, string BadgeText, string ExpiresAtText, string StatusLabel)
{
    public static BenchmarkMetricView ToBenchmarkMetricView(BenchmarkMetric item)
    {
        return new BenchmarkMetricView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Title, 64),
            Text.Truncate(item.Name, 80),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 120),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            item.Badge,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BenchmarkMetricView> ToBenchmarkMetricViews(List<BenchmarkMetric> items)
    {
        var views = new List<BenchmarkMetricView>();
        foreach (var item in items)
        {
            views.Add(ToBenchmarkMetricView(item));
        }
        return views;
    }

    public static double SumBenchmarkMetricView(BenchmarkMetricView view)
    {
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedCeiling + parsedRating;
    }
}
