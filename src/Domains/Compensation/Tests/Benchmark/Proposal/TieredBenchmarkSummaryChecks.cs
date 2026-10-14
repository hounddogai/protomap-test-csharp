namespace Acme.Workforce.Compensation.Tests.Benchmark.Proposal;

using Acme.Workforce.Compensation.Benchmark.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class TieredBenchmarkSummaryChecks
{
    public static void TieredBenchmarkSummaryStartsInRequested()
    {
        var item = TieredBenchmarkSummary.NewTieredBenchmarkSummary("code-943", "name-766");
        Check.Equal(item.Status, TieredBenchmarkSummaryStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredBenchmarkSummaryMovesToGranted()
    {
        var item = TieredBenchmarkSummary.NewTieredBenchmarkSummary("code-943", "name-766");
        try
        {
            item.MoveTo(TieredBenchmarkSummaryStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredBenchmarkSummaryStatus.Granted, "status");
    }

    public static void TieredBenchmarkSummaryRejectsMovesFromExpired()
    {
        var item = TieredBenchmarkSummary.NewTieredBenchmarkSummary("code-943", "name-766");
        item.Status = TieredBenchmarkSummaryStatus.Expired;
        Check.Fails(() => item.MoveTo(TieredBenchmarkSummaryStatus.Requested), "move");
    }

    public static void RunAll()
    {
        TieredBenchmarkSummaryStartsInRequested();
        TieredBenchmarkSummaryMovesToGranted();
        TieredBenchmarkSummaryRejectsMovesFromExpired();
    }
}
