namespace Acme.Workforce.Compensation.Tests.Benchmark.Threshold;

using Acme.Workforce.Compensation.Benchmark.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class BenchmarkPolicyViewChecks
{
    public static void BenchmarkPolicyViewShowsTheStatusInCapitals()
    {
        var item = BenchmarkPolicy.NewBenchmarkPolicy("code-324", "name-385");
        var view = BenchmarkPolicyView.ToBenchmarkPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BenchmarkPolicyView.ToBenchmarkPolicyViews(new List<BenchmarkPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BenchmarkPolicyViewShowsTheStatusInCapitals();
    }
}
