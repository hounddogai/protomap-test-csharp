namespace Acme.Workforce.Compensation.Tests.Benchmark.Rollup;

using Acme.Workforce.Compensation.Benchmark.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class CappedBenchmarkTierViewChecks
{
    public static void CappedBenchmarkTierViewShowsTheStatusInCapitals()
    {
        var item = CappedBenchmarkTier.NewCappedBenchmarkTier("code-809", "label-3");
        var view = CappedBenchmarkTierView.ToCappedBenchmarkTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedBenchmarkTierView.ToCappedBenchmarkTierViews(new List<CappedBenchmarkTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedBenchmarkTierViewShowsTheStatusInCapitals();
    }
}
