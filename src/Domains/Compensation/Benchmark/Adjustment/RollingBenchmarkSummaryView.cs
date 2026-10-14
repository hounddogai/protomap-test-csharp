namespace Acme.Workforce.Compensation.Benchmark.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling benchmark summary.</summary>
public sealed record RollingBenchmarkSummaryView(string CodeText, string Title, string ManagerId, string RetriesText, string Region, string Skill, string Ceiling, string Pinned, string JobCode, string DescriptionText, string StatusLabel)
{
    public static RollingBenchmarkSummaryView ToRollingBenchmarkSummaryView(RollingBenchmarkSummary item)
    {
        return new RollingBenchmarkSummaryView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.ManagerId,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Region,
            item.Skill,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.JobCode,
            item.Description,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingBenchmarkSummaryView> ToRollingBenchmarkSummaryViews(List<RollingBenchmarkSummary> items)
    {
        var views = new List<RollingBenchmarkSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToRollingBenchmarkSummaryView(item));
        }
        return views;
    }

    public static double SumRollingBenchmarkSummaryView(RollingBenchmarkSummaryView view)
    {
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedCeiling;
    }
}
