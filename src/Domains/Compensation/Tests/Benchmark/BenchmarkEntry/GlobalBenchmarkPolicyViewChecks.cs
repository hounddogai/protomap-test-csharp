namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkEntry;

using Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

/// <summary>Checks of benchmark entry code, run by the project's runner.</summary>
public static class GlobalBenchmarkPolicyViewChecks
{
    public static void GlobalBenchmarkPolicyViewShowsTheStatusInCapitals()
    {
        var item = GlobalBenchmarkPolicy.NewGlobalBenchmarkPolicy("code-178", "name-904");
        var view = GlobalBenchmarkPolicyView.ToGlobalBenchmarkPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalBenchmarkPolicyView.ToGlobalBenchmarkPolicyViews(new List<GlobalBenchmarkPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalBenchmarkPolicyViewShowsTheStatusInCapitals();
    }
}
