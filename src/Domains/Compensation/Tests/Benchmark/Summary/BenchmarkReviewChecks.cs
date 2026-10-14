namespace Acme.Workforce.Compensation.Tests.Benchmark.Summary;

using Acme.Workforce.Compensation.Benchmark.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class BenchmarkReviewChecks
{
    public static void BenchmarkReviewStartsInDraft()
    {
        var item = BenchmarkReview.NewBenchmarkReview("code-397", "label-744");
        Check.Equal(item.Status, BenchmarkReviewStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BenchmarkReviewMovesToPending()
    {
        var item = BenchmarkReview.NewBenchmarkReview("code-397", "label-744");
        try
        {
            item.MoveTo(BenchmarkReviewStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BenchmarkReviewStatus.Pending, "status");
    }

    public static void BenchmarkReviewRejectsMovesFromClosed()
    {
        var item = BenchmarkReview.NewBenchmarkReview("code-397", "label-744");
        item.Status = BenchmarkReviewStatus.Closed;
        Check.Fails(() => item.MoveTo(BenchmarkReviewStatus.Draft), "move");
    }

    public static void RunAll()
    {
        BenchmarkReviewStartsInDraft();
        BenchmarkReviewMovesToPending();
        BenchmarkReviewRejectsMovesFromClosed();
    }
}
