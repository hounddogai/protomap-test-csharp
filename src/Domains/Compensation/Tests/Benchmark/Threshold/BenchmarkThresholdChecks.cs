namespace Acme.Workforce.Compensation.Tests.Benchmark.Threshold;

using Acme.Workforce.Compensation.Benchmark.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class BenchmarkThresholdChecks
{
    public static void BenchmarkThresholdStartsInCreated()
    {
        var item = BenchmarkThreshold.NewBenchmarkThreshold("code-628", "label-144");
        Check.Equal(item.Status, BenchmarkThresholdStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BenchmarkThresholdMovesToPublished()
    {
        var item = BenchmarkThreshold.NewBenchmarkThreshold("code-628", "label-144");
        try
        {
            item.MoveTo(BenchmarkThresholdStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BenchmarkThresholdStatus.Published, "status");
    }

    public static void BenchmarkThresholdRejectsMovesFromRetired()
    {
        var item = BenchmarkThreshold.NewBenchmarkThreshold("code-628", "label-144");
        item.Status = BenchmarkThresholdStatus.Retired;
        Check.Fails(() => item.MoveTo(BenchmarkThresholdStatus.Created), "move");
    }

    public static void RunAll()
    {
        BenchmarkThresholdStartsInCreated();
        BenchmarkThresholdMovesToPublished();
        BenchmarkThresholdRejectsMovesFromRetired();
    }
}
