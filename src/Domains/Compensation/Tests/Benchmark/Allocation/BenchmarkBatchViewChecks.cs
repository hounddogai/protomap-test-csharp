namespace Acme.Workforce.Compensation.Tests.Benchmark.Allocation;

using Acme.Workforce.Compensation.Benchmark.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class BenchmarkBatchViewChecks
{
    public static void BenchmarkBatchViewShowsTheStatusInCapitals()
    {
        var item = BenchmarkBatch.NewBenchmarkBatch("code-559", "label-410");
        var view = BenchmarkBatchView.ToBenchmarkBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BenchmarkBatchView.ToBenchmarkBatchViews(new List<BenchmarkBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BenchmarkBatchViewShowsTheStatusInCapitals();
    }
}
