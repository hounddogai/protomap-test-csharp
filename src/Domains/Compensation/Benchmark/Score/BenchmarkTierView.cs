namespace Acme.Workforce.Compensation.Benchmark.Score;

using System.Globalization;

/// <summary>How screens and exports show a benchmark tier.</summary>
public sealed record BenchmarkTierView(string Code, string Title, string OwnerId, string SummaryText, string Score, string LeaveDays, string DescriptionText, string WindowDays, string StatusLabel)
{
    public static BenchmarkTierView ToBenchmarkTierView(BenchmarkTier item)
    {
        return new BenchmarkTierView(
            item.Code,
            item.Title,
            item.OwnerId,
            item.Summary,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Description,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BenchmarkTierView> ToBenchmarkTierViews(List<BenchmarkTier> items)
    {
        var views = new List<BenchmarkTierView>();
        foreach (var item in items)
        {
            views.Add(ToBenchmarkTierView(item));
        }
        return views;
    }

    public static double SumBenchmarkTierView(BenchmarkTierView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedLeaveDays + (double)parsedWindowDays;
    }
}
