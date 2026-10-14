namespace Acme.Workforce.Compensation.Tests.Benchmark.Score;

using Acme.Workforce.Compensation.Benchmark.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class TieredBenchmarkProfileViewChecks
{
    public static void TieredBenchmarkProfileViewShowsTheStatusInCapitals()
    {
        var item = TieredBenchmarkProfile.NewTieredBenchmarkProfile("code-6", "name-357");
        var view = TieredBenchmarkProfileView.ToTieredBenchmarkProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredBenchmarkProfileView.ToTieredBenchmarkProfileViews(new List<TieredBenchmarkProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredBenchmarkProfileViewShowsTheStatusInCapitals();
    }
}
