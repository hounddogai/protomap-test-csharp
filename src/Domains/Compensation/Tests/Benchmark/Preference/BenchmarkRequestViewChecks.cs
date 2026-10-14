namespace Acme.Workforce.Compensation.Tests.Benchmark.Preference;

using Acme.Workforce.Compensation.Benchmark.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class BenchmarkRequestViewChecks
{
    public static void BenchmarkRequestViewShowsTheStatusInCapitals()
    {
        var item = BenchmarkRequest.NewBenchmarkRequest("code-189", "name-374");
        var view = BenchmarkRequestView.ToBenchmarkRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BenchmarkRequestView.ToBenchmarkRequestViews(new List<BenchmarkRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BenchmarkRequestViewShowsTheStatusInCapitals();
    }
}
