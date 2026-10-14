namespace Acme.Workforce.Compensation.Tests.Benchmark.Review;

using Acme.Workforce.Compensation.Benchmark.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RegionalBenchmarkReviewChecks
{
    public static void RegionalBenchmarkReviewStartsInOpen()
    {
        var item = RegionalBenchmarkReview.NewRegionalBenchmarkReview("code-783", "name-846");
        Check.Equal(item.Status, RegionalBenchmarkReviewStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalBenchmarkReviewMovesToResolved()
    {
        var item = RegionalBenchmarkReview.NewRegionalBenchmarkReview("code-783", "name-846");
        try
        {
            item.MoveTo(RegionalBenchmarkReviewStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalBenchmarkReviewStatus.Resolved, "status");
    }

    public static void RegionalBenchmarkReviewRejectsMovesFromArchived()
    {
        var item = RegionalBenchmarkReview.NewRegionalBenchmarkReview("code-783", "name-846");
        item.Status = RegionalBenchmarkReviewStatus.Archived;
        Check.Fails(() => item.MoveTo(RegionalBenchmarkReviewStatus.Open), "move");
    }

    public static void RunAll()
    {
        RegionalBenchmarkReviewStartsInOpen();
        RegionalBenchmarkReviewMovesToResolved();
        RegionalBenchmarkReviewRejectsMovesFromArchived();
    }
}
