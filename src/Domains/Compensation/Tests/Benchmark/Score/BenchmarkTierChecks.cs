namespace Acme.Workforce.Compensation.Tests.Benchmark.Score;

using Acme.Workforce.Compensation.Benchmark.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class BenchmarkTierChecks
{
    public static void BenchmarkTierStartsInOpen()
    {
        var item = BenchmarkTier.NewBenchmarkTier("code-577", "title-933");
        Check.Equal(item.Status, BenchmarkTierStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BenchmarkTierMovesToInReview()
    {
        var item = BenchmarkTier.NewBenchmarkTier("code-577", "title-933");
        try
        {
            item.MoveTo(BenchmarkTierStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BenchmarkTierStatus.InReview, "status");
    }

    public static void BenchmarkTierRejectsMovesFromArchived()
    {
        var item = BenchmarkTier.NewBenchmarkTier("code-577", "title-933");
        item.Status = BenchmarkTierStatus.Archived;
        Check.Fails(() => item.MoveTo(BenchmarkTierStatus.Open), "move");
    }

    public static void RunAll()
    {
        BenchmarkTierStartsInOpen();
        BenchmarkTierMovesToInReview();
        BenchmarkTierRejectsMovesFromArchived();
    }
}
