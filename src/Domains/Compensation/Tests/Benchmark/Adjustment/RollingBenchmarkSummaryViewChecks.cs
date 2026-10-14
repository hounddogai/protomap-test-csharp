namespace Acme.Workforce.Compensation.Tests.Benchmark.Adjustment;

using Acme.Workforce.Compensation.Benchmark.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class RollingBenchmarkSummaryViewChecks
{
    public static void RollingBenchmarkSummaryViewShowsTheStatusInCapitals()
    {
        var item = RollingBenchmarkSummary.NewRollingBenchmarkSummary("code-782", "title-76");
        var view = RollingBenchmarkSummaryView.ToRollingBenchmarkSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingBenchmarkSummaryView.ToRollingBenchmarkSummaryViews(new List<RollingBenchmarkSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingBenchmarkSummaryViewShowsTheStatusInCapitals();
    }
}
