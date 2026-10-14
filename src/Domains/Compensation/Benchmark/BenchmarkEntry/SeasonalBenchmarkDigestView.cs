namespace Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal benchmark digest.</summary>
public sealed record SeasonalBenchmarkDigestView(string Code, string Label, string TimezoneText, string Rate, string StartsAt, string Badge, string LeaveDays, string StatusLabel)
{
    public static SeasonalBenchmarkDigestView ToSeasonalBenchmarkDigestView(SeasonalBenchmarkDigest item)
    {
        return new SeasonalBenchmarkDigestView(
            item.Code,
            item.Label,
            Text.Truncate(item.Timezone, 40),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalBenchmarkDigestView> ToSeasonalBenchmarkDigestViews(List<SeasonalBenchmarkDigest> items)
    {
        var views = new List<SeasonalBenchmarkDigestView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalBenchmarkDigestView(item));
        }
        return views;
    }

    public static double SumSeasonalBenchmarkDigestView(SeasonalBenchmarkDigestView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedStartsAt + (double)parsedLeaveDays;
    }
}
