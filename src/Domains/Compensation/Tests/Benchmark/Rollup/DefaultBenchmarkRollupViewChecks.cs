namespace Acme.Workforce.Compensation.Tests.Benchmark.Rollup;

using Acme.Workforce.Compensation.Benchmark.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class DefaultBenchmarkRollupViewChecks
{
    public static void DefaultBenchmarkRollupViewShowsTheStatusInCapitals()
    {
        var item = DefaultBenchmarkRollup.NewDefaultBenchmarkRollup("code-721", "label-552");
        var view = DefaultBenchmarkRollupView.ToDefaultBenchmarkRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultBenchmarkRollupView.ToDefaultBenchmarkRollupViews(new List<DefaultBenchmarkRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultBenchmarkRollupViewShowsTheStatusInCapitals();
    }
}
