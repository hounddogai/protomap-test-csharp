namespace Acme.Workforce.Compensation.Benchmark.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a benchmark summary.</summary>
public sealed record BenchmarkSummaryView(string Code, string Label, string PinnedText, string HoursText, string BudgetText, string StatusLabel)
{
    public static BenchmarkSummaryView ToBenchmarkSummaryView(BenchmarkSummary item)
    {
        return new BenchmarkSummaryView(
            item.Code,
            Text.Truncate(item.Label, 64),
            $"{item.Pinned}",
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BenchmarkSummaryView> ToBenchmarkSummaryViews(List<BenchmarkSummary> items)
    {
        var views = new List<BenchmarkSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToBenchmarkSummaryView(item));
        }
        return views;
    }

    public static double SumBenchmarkSummaryView(BenchmarkSummaryView view)
    {
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        return parsedHours + parsedBudget;
    }
}
