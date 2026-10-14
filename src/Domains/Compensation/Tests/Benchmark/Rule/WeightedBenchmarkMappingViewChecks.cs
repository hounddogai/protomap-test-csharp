namespace Acme.Workforce.Compensation.Tests.Benchmark.Rule;

using Acme.Workforce.Compensation.Benchmark.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class WeightedBenchmarkMappingViewChecks
{
    public static void WeightedBenchmarkMappingViewShowsTheStatusInCapitals()
    {
        var item = WeightedBenchmarkMapping.NewWeightedBenchmarkMapping("code-565", "title-254");
        var view = WeightedBenchmarkMappingView.ToWeightedBenchmarkMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedBenchmarkMappingView.ToWeightedBenchmarkMappingViews(new List<WeightedBenchmarkMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedBenchmarkMappingViewShowsTheStatusInCapitals();
    }
}
