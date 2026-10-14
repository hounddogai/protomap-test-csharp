namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkEntry;

using Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

/// <summary>Checks of benchmark entry code, run by the project's runner.</summary>
public static class GlobalBenchmarkPolicyChecks
{
    public static void GlobalBenchmarkPolicyStartsInNew()
    {
        var item = GlobalBenchmarkPolicy.NewGlobalBenchmarkPolicy("code-178", "name-904");
        Check.Equal(item.Status, GlobalBenchmarkPolicyStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalBenchmarkPolicyMovesToDone()
    {
        var item = GlobalBenchmarkPolicy.NewGlobalBenchmarkPolicy("code-178", "name-904");
        try
        {
            item.MoveTo(GlobalBenchmarkPolicyStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalBenchmarkPolicyStatus.Done, "status");
    }

    public static void GlobalBenchmarkPolicyRejectsMovesFromFailed()
    {
        var item = GlobalBenchmarkPolicy.NewGlobalBenchmarkPolicy("code-178", "name-904");
        item.Status = GlobalBenchmarkPolicyStatus.Failed;
        Check.Fails(() => item.MoveTo(GlobalBenchmarkPolicyStatus.New), "move");
    }

    public static void RunAll()
    {
        GlobalBenchmarkPolicyStartsInNew();
        GlobalBenchmarkPolicyMovesToDone();
        GlobalBenchmarkPolicyRejectsMovesFromFailed();
    }
}
