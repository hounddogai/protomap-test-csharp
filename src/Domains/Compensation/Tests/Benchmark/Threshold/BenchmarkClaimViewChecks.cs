namespace Acme.Workforce.Compensation.Tests.Benchmark.Threshold;

using Acme.Workforce.Compensation.Benchmark.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class BenchmarkClaimViewChecks
{
    public static void BenchmarkClaimViewShowsTheStatusInCapitals()
    {
        var item = BenchmarkClaim.NewBenchmarkClaim("code-475", "name-807");
        var view = BenchmarkClaimView.ToBenchmarkClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BenchmarkClaimView.ToBenchmarkClaimViews(new List<BenchmarkClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BenchmarkClaimViewShowsTheStatusInCapitals();
    }
}
