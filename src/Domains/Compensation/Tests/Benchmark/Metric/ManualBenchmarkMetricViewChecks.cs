namespace Acme.Workforce.Compensation.Tests.Benchmark.Metric;

using Acme.Workforce.Compensation.Benchmark.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ManualBenchmarkMetricViewChecks
{
    public static void ManualBenchmarkMetricViewShowsTheStatusInCapitals()
    {
        var item = ManualBenchmarkMetric.NewManualBenchmarkMetric("code-971", "label-834");
        var view = ManualBenchmarkMetricView.ToManualBenchmarkMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualBenchmarkMetricView.ToManualBenchmarkMetricViews(new List<ManualBenchmarkMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualBenchmarkMetricViewShowsTheStatusInCapitals();
    }
}
