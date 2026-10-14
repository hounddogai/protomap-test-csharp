namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkSummary;

using Acme.Workforce.Compensation.Benchmark.BenchmarkSummary;

/// <summary>Checks of benchmark summary code, run by the project's runner.</summary>
public static class GlobalBenchmarkOverrideChecks
{
    public static void GlobalBenchmarkOverrideStartsInOpen()
    {
        var item = GlobalBenchmarkOverride.NewGlobalBenchmarkOverride("code-758", "label-28");
        Check.Equal(item.Status, GlobalBenchmarkOverrideStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalBenchmarkOverrideMovesToInReview()
    {
        var item = GlobalBenchmarkOverride.NewGlobalBenchmarkOverride("code-758", "label-28");
        try
        {
            item.MoveTo(GlobalBenchmarkOverrideStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalBenchmarkOverrideStatus.InReview, "status");
    }

    public static void GlobalBenchmarkOverrideRejectsMovesFromArchived()
    {
        var item = GlobalBenchmarkOverride.NewGlobalBenchmarkOverride("code-758", "label-28");
        item.Status = GlobalBenchmarkOverrideStatus.Archived;
        Check.Fails(() => item.MoveTo(GlobalBenchmarkOverrideStatus.Open), "move");
    }

    public static void RunAll()
    {
        GlobalBenchmarkOverrideStartsInOpen();
        GlobalBenchmarkOverrideMovesToInReview();
        GlobalBenchmarkOverrideRejectsMovesFromArchived();
    }
}
