namespace Acme.Workforce.Compensation.Tests.Benchmark.Summary;

using Acme.Workforce.Compensation.Benchmark.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class BenchmarkSummaryViewChecks
{
    public static void BenchmarkSummaryViewShowsTheStatusInCapitals()
    {
        var item = BenchmarkSummary.NewBenchmarkSummary("code-520", "label-137");
        var view = BenchmarkSummaryView.ToBenchmarkSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BenchmarkSummaryView.ToBenchmarkSummaryViews(new List<BenchmarkSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BenchmarkSummaryViewShowsTheStatusInCapitals();
    }
}
