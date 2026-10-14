namespace Acme.Workforce.Compensation.Tests.Benchmark.Adjustment;

using Acme.Workforce.Compensation.Benchmark.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class RollingBenchmarkSummaryChecks
{
    public static void RollingBenchmarkSummaryStartsInNew()
    {
        var item = RollingBenchmarkSummary.NewRollingBenchmarkSummary("code-782", "title-76");
        Check.Equal(item.Status, RollingBenchmarkSummaryStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingBenchmarkSummaryMovesToRunning()
    {
        var item = RollingBenchmarkSummary.NewRollingBenchmarkSummary("code-782", "title-76");
        try
        {
            item.MoveTo(RollingBenchmarkSummaryStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingBenchmarkSummaryStatus.Running, "status");
    }

    public static void RollingBenchmarkSummaryRejectsMovesFromFailed()
    {
        var item = RollingBenchmarkSummary.NewRollingBenchmarkSummary("code-782", "title-76");
        item.Status = RollingBenchmarkSummaryStatus.Failed;
        Check.Fails(() => item.MoveTo(RollingBenchmarkSummaryStatus.New), "move");
    }

    public static void RunAll()
    {
        RollingBenchmarkSummaryStartsInNew();
        RollingBenchmarkSummaryMovesToRunning();
        RollingBenchmarkSummaryRejectsMovesFromFailed();
    }
}
