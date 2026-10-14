namespace Acme.Workforce.Compensation.Tests.Benchmark.Threshold;

using Acme.Workforce.Compensation.Benchmark.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class BenchmarkPolicyChecks
{
    public static void BenchmarkPolicyStartsInCreated()
    {
        var item = BenchmarkPolicy.NewBenchmarkPolicy("code-324", "name-385");
        Check.Equal(item.Status, BenchmarkPolicyStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BenchmarkPolicyMovesToPublished()
    {
        var item = BenchmarkPolicy.NewBenchmarkPolicy("code-324", "name-385");
        try
        {
            item.MoveTo(BenchmarkPolicyStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BenchmarkPolicyStatus.Published, "status");
    }

    public static void BenchmarkPolicyRejectsMovesFromRetired()
    {
        var item = BenchmarkPolicy.NewBenchmarkPolicy("code-324", "name-385");
        item.Status = BenchmarkPolicyStatus.Retired;
        Check.Fails(() => item.MoveTo(BenchmarkPolicyStatus.Created), "move");
    }

    public static void RunAll()
    {
        BenchmarkPolicyStartsInCreated();
        BenchmarkPolicyMovesToPublished();
        BenchmarkPolicyRejectsMovesFromRetired();
    }
}
