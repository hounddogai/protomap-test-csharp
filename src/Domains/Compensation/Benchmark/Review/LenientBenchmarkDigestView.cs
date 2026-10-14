namespace Acme.Workforce.Compensation.Benchmark.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient benchmark digest.</summary>
public sealed record LenientBenchmarkDigestView(string Code, string Title, string FloorValue, string Rate, string SourceText, string SkillText, string Segment, string StatusLabel)
{
    public static LenientBenchmarkDigestView ToLenientBenchmarkDigestView(LenientBenchmarkDigest item)
    {
        return new LenientBenchmarkDigestView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Source,
            item.Skill,
            item.Segment,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientBenchmarkDigestView> ToLenientBenchmarkDigestViews(List<LenientBenchmarkDigest> items)
    {
        var views = new List<LenientBenchmarkDigestView>();
        foreach (var item in items)
        {
            views.Add(ToLenientBenchmarkDigestView(item));
        }
        return views;
    }

    public static double SumLenientBenchmarkDigestView(LenientBenchmarkDigestView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return parsedFloorValue + parsedRate;
    }
}
