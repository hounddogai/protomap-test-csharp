namespace Acme.Workforce.Compensation.Tests.Benchmark.Score;

using Acme.Workforce.Compensation.Benchmark.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class TieredBenchmarkScoreChecks
{
    public static void TieredBenchmarkScoreStartsInProposed()
    {
        var item = TieredBenchmarkScore.NewTieredBenchmarkScore("code-598", "name-864");
        Check.Equal(item.Status, TieredBenchmarkScoreStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredBenchmarkScoreMovesToApproved()
    {
        var item = TieredBenchmarkScore.NewTieredBenchmarkScore("code-598", "name-864");
        try
        {
            item.MoveTo(TieredBenchmarkScoreStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredBenchmarkScoreStatus.Approved, "status");
    }

    public static void TieredBenchmarkScoreRejectsMovesFromWithdrawn()
    {
        var item = TieredBenchmarkScore.NewTieredBenchmarkScore("code-598", "name-864");
        item.Status = TieredBenchmarkScoreStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(TieredBenchmarkScoreStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        TieredBenchmarkScoreStartsInProposed();
        TieredBenchmarkScoreMovesToApproved();
        TieredBenchmarkScoreRejectsMovesFromWithdrawn();
    }
}
