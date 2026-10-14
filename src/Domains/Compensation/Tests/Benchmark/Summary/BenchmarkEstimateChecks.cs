namespace Acme.Workforce.Compensation.Tests.Benchmark.Summary;

using Acme.Workforce.Compensation.Benchmark.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class BenchmarkEstimateChecks
{
    public static void BenchmarkEstimateStartsInIdle()
    {
        var item = BenchmarkEstimate.NewBenchmarkEstimate("code-936", "title-175");
        Check.Equal(item.Status, BenchmarkEstimateStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BenchmarkEstimateMovesToWarming()
    {
        var item = BenchmarkEstimate.NewBenchmarkEstimate("code-936", "title-175");
        try
        {
            item.MoveTo(BenchmarkEstimateStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BenchmarkEstimateStatus.Warming, "status");
    }

    public static void BenchmarkEstimateRejectsMovesFromStopped()
    {
        var item = BenchmarkEstimate.NewBenchmarkEstimate("code-936", "title-175");
        item.Status = BenchmarkEstimateStatus.Stopped;
        Check.Fails(() => item.MoveTo(BenchmarkEstimateStatus.Idle), "move");
    }

    public static void RunAll()
    {
        BenchmarkEstimateStartsInIdle();
        BenchmarkEstimateMovesToWarming();
        BenchmarkEstimateRejectsMovesFromStopped();
    }
}
