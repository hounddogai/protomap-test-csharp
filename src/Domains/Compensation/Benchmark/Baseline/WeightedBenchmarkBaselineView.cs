namespace Acme.Workforce.Compensation.Benchmark.Baseline;

using System.Globalization;

/// <summary>How screens and exports show a weighted benchmark baseline.</summary>
public sealed record WeightedBenchmarkBaselineView(string Code, string TitleText, string CostCenter, string Skill, string SegmentText, string Ratio, string Pinned, string Cost, string DurationMinutes, string TenureMonthsText, string StatusLabel)
{
    public static WeightedBenchmarkBaselineView ToWeightedBenchmarkBaselineView(WeightedBenchmarkBaseline item)
    {
        return new WeightedBenchmarkBaselineView(
            item.Code,
            item.Title,
            item.CostCenter,
            item.Skill,
            item.Segment,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedBenchmarkBaselineView> ToWeightedBenchmarkBaselineViews(List<WeightedBenchmarkBaseline> items)
    {
        var views = new List<WeightedBenchmarkBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedBenchmarkBaselineView(item));
        }
        return views;
    }

    public static double SumWeightedBenchmarkBaselineView(WeightedBenchmarkBaselineView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return parsedRatio + parsedCost + (double)parsedDurationMinutes;
    }
}
