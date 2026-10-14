namespace Acme.Workforce.Compensation.Benchmark.Preference;

using System.Globalization;

/// <summary>How screens and exports show a benchmark segment.</summary>
public sealed record BenchmarkSegmentView(string Code, string LabelText, string Retries, string ChecksumText, string WindowDaysText, string ScoreText, string StatusLabel)
{
    public static BenchmarkSegmentView ToBenchmarkSegmentView(BenchmarkSegment item)
    {
        return new BenchmarkSegmentView(
            item.Code,
            item.Label,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BenchmarkSegmentView> ToBenchmarkSegmentViews(List<BenchmarkSegment> items)
    {
        var views = new List<BenchmarkSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToBenchmarkSegmentView(item));
        }
        return views;
    }

    public static double SumBenchmarkSegmentView(BenchmarkSegmentView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedWindowDays + parsedScore;
    }
}
