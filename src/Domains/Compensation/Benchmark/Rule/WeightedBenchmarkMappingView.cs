namespace Acme.Workforce.Compensation.Benchmark.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted benchmark mapping.</summary>
public sealed record WeightedBenchmarkMappingView(string Code, string Title, string ThresholdText, string GraceDays, string Country, string OwnerId, string StatusLabel)
{
    public static WeightedBenchmarkMappingView ToWeightedBenchmarkMappingView(WeightedBenchmarkMapping item)
    {
        return new WeightedBenchmarkMappingView(
            Text.Truncate(item.Code, 80),
            item.Title,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Country,
            item.OwnerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedBenchmarkMappingView> ToWeightedBenchmarkMappingViews(List<WeightedBenchmarkMapping> items)
    {
        var views = new List<WeightedBenchmarkMappingView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedBenchmarkMappingView(item));
        }
        return views;
    }

    public static double SumWeightedBenchmarkMappingView(WeightedBenchmarkMappingView view)
    {
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedGraceDays;
    }
}
