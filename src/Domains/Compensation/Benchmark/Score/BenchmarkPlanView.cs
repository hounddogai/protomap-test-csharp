namespace Acme.Workforce.Compensation.Benchmark.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a benchmark plan.</summary>
public sealed record BenchmarkPlanView(string Code, string Label, string SourceText, string JobCodeText, string ReviewerText, string MinimumText, string ThresholdText, string Capacity, string Name, string StatusLabel)
{
    public static BenchmarkPlanView ToBenchmarkPlanView(BenchmarkPlan item)
    {
        return new BenchmarkPlanView(
            Text.Truncate(item.Code, 80),
            item.Label,
            Text.Truncate(item.Source, 64),
            item.JobCode,
            item.Reviewer,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Name, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BenchmarkPlanView> ToBenchmarkPlanViews(List<BenchmarkPlan> items)
    {
        var views = new List<BenchmarkPlanView>();
        foreach (var item in items)
        {
            views.Add(ToBenchmarkPlanView(item));
        }
        return views;
    }

    public static double SumBenchmarkPlanView(BenchmarkPlanView view)
    {
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedThreshold + (double)parsedCapacity;
    }
}
