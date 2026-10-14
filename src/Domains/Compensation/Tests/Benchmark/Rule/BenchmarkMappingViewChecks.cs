namespace Acme.Workforce.Compensation.Tests.Benchmark.Rule;

using Acme.Workforce.Compensation.Benchmark.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class BenchmarkMappingViewChecks
{
    public static void BenchmarkMappingViewShowsTheStatusInCapitals()
    {
        var item = BenchmarkMapping.NewBenchmarkMapping("code-594", "name-416");
        var view = BenchmarkMappingView.ToBenchmarkMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BenchmarkMappingView.ToBenchmarkMappingViews(new List<BenchmarkMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BenchmarkMappingViewShowsTheStatusInCapitals();
    }
}
