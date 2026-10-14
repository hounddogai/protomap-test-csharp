namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkRule;

using Acme.Workforce.Compensation.Benchmark.BenchmarkRule;

/// <summary>Checks of benchmark rule code, run by the project's runner.</summary>
public static class BenchmarkRollupViewChecks
{
    public static void BenchmarkRollupViewShowsTheStatusInCapitals()
    {
        var item = BenchmarkRollup.NewBenchmarkRollup("code-598", "name-463");
        var view = BenchmarkRollupView.ToBenchmarkRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BenchmarkRollupView.ToBenchmarkRollupViews(new List<BenchmarkRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BenchmarkRollupViewShowsTheStatusInCapitals();
    }
}
