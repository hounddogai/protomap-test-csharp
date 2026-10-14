namespace Acme.Workforce.Compensation.Tests.Benchmark.Summary;

using Acme.Workforce.Compensation.Benchmark.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class BenchmarkOverrideChecks
{
    public static void BenchmarkOverrideStartsInIdle()
    {
        var item = BenchmarkOverride.NewBenchmarkOverride("code-331", "title-341");
        Check.Equal(item.Status, BenchmarkOverrideStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BenchmarkOverrideMovesToWarming()
    {
        var item = BenchmarkOverride.NewBenchmarkOverride("code-331", "title-341");
        try
        {
            item.MoveTo(BenchmarkOverrideStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BenchmarkOverrideStatus.Warming, "status");
    }

    public static void BenchmarkOverrideRejectsMovesFromStopped()
    {
        var item = BenchmarkOverride.NewBenchmarkOverride("code-331", "title-341");
        item.Status = BenchmarkOverrideStatus.Stopped;
        Check.Fails(() => item.MoveTo(BenchmarkOverrideStatus.Idle), "move");
    }

    public static void RunAll()
    {
        BenchmarkOverrideStartsInIdle();
        BenchmarkOverrideMovesToWarming();
        BenchmarkOverrideRejectsMovesFromStopped();
    }
}
