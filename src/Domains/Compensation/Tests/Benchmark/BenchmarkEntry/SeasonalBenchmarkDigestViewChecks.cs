namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkEntry;

using Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

/// <summary>Checks of benchmark entry code, run by the project's runner.</summary>
public static class SeasonalBenchmarkDigestViewChecks
{
    public static void SeasonalBenchmarkDigestViewShowsTheStatusInCapitals()
    {
        var item = SeasonalBenchmarkDigest.NewSeasonalBenchmarkDigest("code-878", "label-554");
        var view = SeasonalBenchmarkDigestView.ToSeasonalBenchmarkDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalBenchmarkDigestView.ToSeasonalBenchmarkDigestViews(new List<SeasonalBenchmarkDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalBenchmarkDigestViewShowsTheStatusInCapitals();
    }
}
