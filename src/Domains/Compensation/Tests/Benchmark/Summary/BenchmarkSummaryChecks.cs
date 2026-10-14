namespace Acme.Workforce.Compensation.Tests.Benchmark.Summary;

using Acme.Workforce.Compensation.Benchmark.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class BenchmarkSummaryChecks
{
    public static void BenchmarkSummaryStartsInNew()
    {
        var item = BenchmarkSummary.NewBenchmarkSummary("code-520", "label-137");
        Check.Equal(item.Status, BenchmarkSummaryStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BenchmarkSummaryMovesToDone()
    {
        var item = BenchmarkSummary.NewBenchmarkSummary("code-520", "label-137");
        try
        {
            item.MoveTo(BenchmarkSummaryStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BenchmarkSummaryStatus.Done, "status");
    }

    public static void BenchmarkSummaryRejectsMovesFromFailed()
    {
        var item = BenchmarkSummary.NewBenchmarkSummary("code-520", "label-137");
        item.Status = BenchmarkSummaryStatus.Failed;
        Check.Fails(() => item.MoveTo(BenchmarkSummaryStatus.New), "move");
    }

    public static void RunAll()
    {
        BenchmarkSummaryStartsInNew();
        BenchmarkSummaryMovesToDone();
        BenchmarkSummaryRejectsMovesFromFailed();
    }
}
