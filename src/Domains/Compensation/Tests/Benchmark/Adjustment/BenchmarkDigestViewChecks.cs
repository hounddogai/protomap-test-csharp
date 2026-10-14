namespace Acme.Workforce.Compensation.Tests.Benchmark.Adjustment;

using Acme.Workforce.Compensation.Benchmark.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class BenchmarkDigestViewChecks
{
    public static void BenchmarkDigestViewShowsTheStatusInCapitals()
    {
        var item = BenchmarkDigest.NewBenchmarkDigest("code-48", "title-828");
        var view = BenchmarkDigestView.ToBenchmarkDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BenchmarkDigestView.ToBenchmarkDigestViews(new List<BenchmarkDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BenchmarkDigestViewShowsTheStatusInCapitals();
    }
}
