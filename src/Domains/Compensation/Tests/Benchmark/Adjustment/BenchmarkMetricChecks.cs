namespace Acme.Workforce.Compensation.Tests.Benchmark.Adjustment;

using Acme.Workforce.Compensation.Benchmark.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class BenchmarkMetricChecks
{
    public static void BenchmarkMetricStartsInDraft()
    {
        var item = BenchmarkMetric.NewBenchmarkMetric("code-508", "title-533");
        Check.Equal(item.Status, BenchmarkMetricStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BenchmarkMetricMovesToClosed()
    {
        var item = BenchmarkMetric.NewBenchmarkMetric("code-508", "title-533");
        try
        {
            item.MoveTo(BenchmarkMetricStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BenchmarkMetricStatus.Closed, "status");
    }

    public static void BenchmarkMetricRejectsMovesFromClosed()
    {
        var item = BenchmarkMetric.NewBenchmarkMetric("code-508", "title-533");
        item.Status = BenchmarkMetricStatus.Closed;
        Check.Fails(() => item.MoveTo(BenchmarkMetricStatus.Draft), "move");
    }

    public static void RunAll()
    {
        BenchmarkMetricStartsInDraft();
        BenchmarkMetricMovesToClosed();
        BenchmarkMetricRejectsMovesFromClosed();
    }
}
