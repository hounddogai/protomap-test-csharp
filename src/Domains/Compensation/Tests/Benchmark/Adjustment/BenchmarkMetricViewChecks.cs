namespace Acme.Workforce.Compensation.Tests.Benchmark.Adjustment;

using Acme.Workforce.Compensation.Benchmark.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class BenchmarkMetricViewChecks
{
    public static void BenchmarkMetricViewShowsTheStatusInCapitals()
    {
        var item = BenchmarkMetric.NewBenchmarkMetric("code-508", "title-533");
        var view = BenchmarkMetricView.ToBenchmarkMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BenchmarkMetricView.ToBenchmarkMetricViews(new List<BenchmarkMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BenchmarkMetricViewShowsTheStatusInCapitals();
    }
}
