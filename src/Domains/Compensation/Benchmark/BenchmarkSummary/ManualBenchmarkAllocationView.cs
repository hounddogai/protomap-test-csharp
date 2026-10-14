namespace Acme.Workforce.Compensation.Benchmark.BenchmarkSummary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual benchmark allocation.</summary>
public sealed record ManualBenchmarkAllocationView(string CodeText, string Label, string Summary, string Location, string Cost, string Skill, string Grade, string Retries, string StartsAtText, string TimezoneText, string StatusLabel)
{
    public static ManualBenchmarkAllocationView ToManualBenchmarkAllocationView(ManualBenchmarkAllocation item)
    {
        return new ManualBenchmarkAllocationView(
            item.Code,
            item.Label,
            item.Summary,
            Text.Truncate(item.Location, 80),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.Grade,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualBenchmarkAllocationView> ToManualBenchmarkAllocationViews(List<ManualBenchmarkAllocation> items)
    {
        var views = new List<ManualBenchmarkAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToManualBenchmarkAllocationView(item));
        }
        return views;
    }

    public static double SumManualBenchmarkAllocationView(ManualBenchmarkAllocationView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        return parsedCost + (double)parsedRetries + (double)parsedStartsAt;
    }
}
