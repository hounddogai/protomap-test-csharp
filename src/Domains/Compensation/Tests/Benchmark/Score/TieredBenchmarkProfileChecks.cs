namespace Acme.Workforce.Compensation.Tests.Benchmark.Score;

using Acme.Workforce.Compensation.Benchmark.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class TieredBenchmarkProfileChecks
{
    public static void TieredBenchmarkProfileStartsInPlanned()
    {
        var item = TieredBenchmarkProfile.NewTieredBenchmarkProfile("code-6", "name-357");
        Check.Equal(item.Status, TieredBenchmarkProfileStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredBenchmarkProfileMovesToInProgress()
    {
        var item = TieredBenchmarkProfile.NewTieredBenchmarkProfile("code-6", "name-357");
        try
        {
            item.MoveTo(TieredBenchmarkProfileStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredBenchmarkProfileStatus.InProgress, "status");
    }

    public static void TieredBenchmarkProfileRejectsMovesFromCancelled()
    {
        var item = TieredBenchmarkProfile.NewTieredBenchmarkProfile("code-6", "name-357");
        item.Status = TieredBenchmarkProfileStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TieredBenchmarkProfileStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TieredBenchmarkProfileStartsInPlanned();
        TieredBenchmarkProfileMovesToInProgress();
        TieredBenchmarkProfileRejectsMovesFromCancelled();
    }
}
