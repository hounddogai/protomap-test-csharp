namespace Acme.Workforce.Compensation.Tests.Benchmark.Threshold;

using Acme.Workforce.Compensation.Benchmark.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class BenchmarkClaimChecks
{
    public static void BenchmarkClaimStartsInNew()
    {
        var item = BenchmarkClaim.NewBenchmarkClaim("code-475", "name-807");
        Check.Equal(item.Status, BenchmarkClaimStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BenchmarkClaimMovesToFailed()
    {
        var item = BenchmarkClaim.NewBenchmarkClaim("code-475", "name-807");
        try
        {
            item.MoveTo(BenchmarkClaimStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BenchmarkClaimStatus.Failed, "status");
    }

    public static void BenchmarkClaimRejectsMovesFromFailed()
    {
        var item = BenchmarkClaim.NewBenchmarkClaim("code-475", "name-807");
        item.Status = BenchmarkClaimStatus.Failed;
        Check.Fails(() => item.MoveTo(BenchmarkClaimStatus.New), "move");
    }

    public static void RunAll()
    {
        BenchmarkClaimStartsInNew();
        BenchmarkClaimMovesToFailed();
        BenchmarkClaimRejectsMovesFromFailed();
    }
}
