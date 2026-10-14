namespace Acme.Workforce.Compensation.Tests.Benchmark.Review;

using Acme.Workforce.Compensation.Benchmark.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class LenientBenchmarkRollupViewChecks
{
    public static void LenientBenchmarkRollupViewShowsTheStatusInCapitals()
    {
        var item = LenientBenchmarkRollup.NewLenientBenchmarkRollup("code-452", "title-11");
        var view = LenientBenchmarkRollupView.ToLenientBenchmarkRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientBenchmarkRollupView.ToLenientBenchmarkRollupViews(new List<LenientBenchmarkRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientBenchmarkRollupViewShowsTheStatusInCapitals();
    }
}
