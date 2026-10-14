namespace Acme.Workforce.Compensation.Tests.Benchmark.Rule;

using Acme.Workforce.Compensation.Benchmark.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class BenchmarkRuleViewChecks
{
    public static void BenchmarkRuleViewShowsTheStatusInCapitals()
    {
        var item = BenchmarkRule.NewBenchmarkRule("code-174", "label-946");
        var view = BenchmarkRuleView.ToBenchmarkRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BenchmarkRuleView.ToBenchmarkRuleViews(new List<BenchmarkRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BenchmarkRuleViewShowsTheStatusInCapitals();
    }
}
