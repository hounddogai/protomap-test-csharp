namespace Acme.Workforce.Compensation.Tests.Benchmark.Allocation;

using Acme.Workforce.Compensation.Benchmark.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class BenchmarkBatchChecks
{
    public static void BenchmarkBatchStartsInOpen()
    {
        var item = BenchmarkBatch.NewBenchmarkBatch("code-559", "label-410");
        Check.Equal(item.Status, BenchmarkBatchStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BenchmarkBatchMovesToInReview()
    {
        var item = BenchmarkBatch.NewBenchmarkBatch("code-559", "label-410");
        try
        {
            item.MoveTo(BenchmarkBatchStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BenchmarkBatchStatus.InReview, "status");
    }

    public static void BenchmarkBatchRejectsMovesFromArchived()
    {
        var item = BenchmarkBatch.NewBenchmarkBatch("code-559", "label-410");
        item.Status = BenchmarkBatchStatus.Archived;
        Check.Fails(() => item.MoveTo(BenchmarkBatchStatus.Open), "move");
    }

    public static void RunAll()
    {
        BenchmarkBatchStartsInOpen();
        BenchmarkBatchMovesToInReview();
        BenchmarkBatchRejectsMovesFromArchived();
    }
}
