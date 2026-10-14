namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkSummary;

using Acme.Workforce.Compensation.Benchmark.BenchmarkSummary;

/// <summary>Checks of benchmark summary code, run by the project's runner.</summary>
public static class SeasonalBenchmarkSummaryChecks
{
    public static void SeasonalBenchmarkSummaryStartsInPlanned()
    {
        var item = SeasonalBenchmarkSummary.NewSeasonalBenchmarkSummary("code-378", "name-674");
        Check.Equal(item.Status, SeasonalBenchmarkSummaryStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalBenchmarkSummaryMovesToCompleted()
    {
        var item = SeasonalBenchmarkSummary.NewSeasonalBenchmarkSummary("code-378", "name-674");
        try
        {
            item.MoveTo(SeasonalBenchmarkSummaryStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalBenchmarkSummaryStatus.Completed, "status");
    }

    public static void SeasonalBenchmarkSummaryRejectsMovesFromCancelled()
    {
        var item = SeasonalBenchmarkSummary.NewSeasonalBenchmarkSummary("code-378", "name-674");
        item.Status = SeasonalBenchmarkSummaryStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SeasonalBenchmarkSummaryStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SeasonalBenchmarkSummaryStartsInPlanned();
        SeasonalBenchmarkSummaryMovesToCompleted();
        SeasonalBenchmarkSummaryRejectsMovesFromCancelled();
    }
}
