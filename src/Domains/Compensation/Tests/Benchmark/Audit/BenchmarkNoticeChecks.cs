namespace Acme.Workforce.Compensation.Tests.Benchmark.Audit;

using Acme.Workforce.Compensation.Benchmark.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class BenchmarkNoticeChecks
{
    public static void BenchmarkNoticeStartsInNew()
    {
        var item = BenchmarkNotice.NewBenchmarkNotice("code-470", "title-639");
        Check.Equal(item.Status, BenchmarkNoticeStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BenchmarkNoticeMovesToQueued()
    {
        var item = BenchmarkNotice.NewBenchmarkNotice("code-470", "title-639");
        try
        {
            item.MoveTo(BenchmarkNoticeStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BenchmarkNoticeStatus.Queued, "status");
    }

    public static void BenchmarkNoticeRejectsMovesFromFailed()
    {
        var item = BenchmarkNotice.NewBenchmarkNotice("code-470", "title-639");
        item.Status = BenchmarkNoticeStatus.Failed;
        Check.Fails(() => item.MoveTo(BenchmarkNoticeStatus.New), "move");
    }

    public static void RunAll()
    {
        BenchmarkNoticeStartsInNew();
        BenchmarkNoticeMovesToQueued();
        BenchmarkNoticeRejectsMovesFromFailed();
    }
}
