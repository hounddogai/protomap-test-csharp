namespace Acme.Workforce.Compensation.Tests.Benchmark.Score;

using Acme.Workforce.Compensation.Benchmark.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class BenchmarkTierViewChecks
{
    public static void BenchmarkTierViewShowsTheStatusInCapitals()
    {
        var item = BenchmarkTier.NewBenchmarkTier("code-577", "title-933");
        var view = BenchmarkTierView.ToBenchmarkTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BenchmarkTierView.ToBenchmarkTierViews(new List<BenchmarkTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BenchmarkTierViewShowsTheStatusInCapitals();
    }
}
