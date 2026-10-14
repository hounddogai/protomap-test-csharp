namespace Acme.Workforce.Compensation.Tests.Benchmark.Milestone;

using Acme.Workforce.Compensation.Benchmark.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class BenchmarkWindowViewChecks
{
    public static void BenchmarkWindowViewShowsTheStatusInCapitals()
    {
        var item = BenchmarkWindow.NewBenchmarkWindow("code-866", "name-287");
        var view = BenchmarkWindowView.ToBenchmarkWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BenchmarkWindowView.ToBenchmarkWindowViews(new List<BenchmarkWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BenchmarkWindowViewShowsTheStatusInCapitals();
    }
}
