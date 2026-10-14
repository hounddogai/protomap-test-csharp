namespace Acme.Workforce.Compensation.Tests.Benchmark.Adjustment;

using Acme.Workforce.Compensation.Benchmark.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class BenchmarkDigestChecks
{
    public static void BenchmarkDigestStartsInRequested()
    {
        var item = BenchmarkDigest.NewBenchmarkDigest("code-48", "title-828");
        Check.Equal(item.Status, BenchmarkDigestStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BenchmarkDigestMovesToGranted()
    {
        var item = BenchmarkDigest.NewBenchmarkDigest("code-48", "title-828");
        try
        {
            item.MoveTo(BenchmarkDigestStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BenchmarkDigestStatus.Granted, "status");
    }

    public static void BenchmarkDigestRejectsMovesFromExpired()
    {
        var item = BenchmarkDigest.NewBenchmarkDigest("code-48", "title-828");
        item.Status = BenchmarkDigestStatus.Expired;
        Check.Fails(() => item.MoveTo(BenchmarkDigestStatus.Requested), "move");
    }

    public static void RunAll()
    {
        BenchmarkDigestStartsInRequested();
        BenchmarkDigestMovesToGranted();
        BenchmarkDigestRejectsMovesFromExpired();
    }
}
