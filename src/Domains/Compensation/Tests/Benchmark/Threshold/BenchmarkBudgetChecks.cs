namespace Acme.Workforce.Compensation.Tests.Benchmark.Threshold;

using Acme.Workforce.Compensation.Benchmark.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class BenchmarkBudgetChecks
{
    public static void BenchmarkBudgetStartsInPlanned()
    {
        var item = BenchmarkBudget.NewBenchmarkBudget("code-521", "title-629");
        Check.Equal(item.Status, BenchmarkBudgetStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BenchmarkBudgetMovesToInProgress()
    {
        var item = BenchmarkBudget.NewBenchmarkBudget("code-521", "title-629");
        try
        {
            item.MoveTo(BenchmarkBudgetStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BenchmarkBudgetStatus.InProgress, "status");
    }

    public static void BenchmarkBudgetRejectsMovesFromCancelled()
    {
        var item = BenchmarkBudget.NewBenchmarkBudget("code-521", "title-629");
        item.Status = BenchmarkBudgetStatus.Cancelled;
        Check.Fails(() => item.MoveTo(BenchmarkBudgetStatus.Planned), "move");
    }

    public static void RunAll()
    {
        BenchmarkBudgetStartsInPlanned();
        BenchmarkBudgetMovesToInProgress();
        BenchmarkBudgetRejectsMovesFromCancelled();
    }
}
