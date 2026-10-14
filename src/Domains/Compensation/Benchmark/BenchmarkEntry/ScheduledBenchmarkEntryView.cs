namespace Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled benchmark entry.</summary>
public sealed record ScheduledBenchmarkEntryView(string Code, string LabelText, string Phone, string Retries, string Reviewer, string StatusLabel)
{
    public static ScheduledBenchmarkEntryView ToScheduledBenchmarkEntryView(ScheduledBenchmarkEntry item)
    {
        return new ScheduledBenchmarkEntryView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.Phone,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledBenchmarkEntryView> ToScheduledBenchmarkEntryViews(List<ScheduledBenchmarkEntry> items)
    {
        var views = new List<ScheduledBenchmarkEntryView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledBenchmarkEntryView(item));
        }
        return views;
    }

    public static double SumScheduledBenchmarkEntryView(ScheduledBenchmarkEntryView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedRetries;
    }
}
