namespace Acme.Workforce.Compensation.Benchmark.BenchmarkAudit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a benchmark schedule.</summary>
public sealed record BenchmarkScheduleView(string Code, string LabelText, string Priority, string Channel, string Rating, string Locale, string StatusLabel)
{
    public static BenchmarkScheduleView ToBenchmarkScheduleView(BenchmarkSchedule item)
    {
        return new BenchmarkScheduleView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 80),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BenchmarkScheduleView> ToBenchmarkScheduleViews(List<BenchmarkSchedule> items)
    {
        var views = new List<BenchmarkScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToBenchmarkScheduleView(item));
        }
        return views;
    }

    public static double SumBenchmarkScheduleView(BenchmarkScheduleView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return (double)parsedPriority + parsedRating;
    }
}
