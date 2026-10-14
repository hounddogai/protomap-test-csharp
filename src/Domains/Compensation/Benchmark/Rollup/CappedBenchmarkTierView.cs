namespace Acme.Workforce.Compensation.Benchmark.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped benchmark tier.</summary>
public sealed record CappedBenchmarkTierView(string Code, string Label, string HoursText, string Rating, string Attempts, string Grade, string StatusLabel)
{
    public static CappedBenchmarkTierView ToCappedBenchmarkTierView(CappedBenchmarkTier item)
    {
        return new CappedBenchmarkTierView(
            item.Code,
            item.Label,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedBenchmarkTierView> ToCappedBenchmarkTierViews(List<CappedBenchmarkTier> items)
    {
        var views = new List<CappedBenchmarkTierView>();
        foreach (var item in items)
        {
            views.Add(ToCappedBenchmarkTierView(item));
        }
        return views;
    }

    public static double SumCappedBenchmarkTierView(CappedBenchmarkTierView view)
    {
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return parsedHours + parsedRating + (double)parsedAttempts;
    }
}
