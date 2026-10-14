namespace Acme.Workforce.Compensation.Tests.Benchmark.Rule;

using Acme.Workforce.Compensation.Benchmark.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class BenchmarkAdjustmentViewChecks
{
    public static void BenchmarkAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = BenchmarkAdjustment.NewBenchmarkAdjustment("code-319", "label-336");
        var view = BenchmarkAdjustmentView.ToBenchmarkAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BenchmarkAdjustmentView.ToBenchmarkAdjustmentViews(new List<BenchmarkAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BenchmarkAdjustmentViewShowsTheStatusInCapitals();
    }
}
