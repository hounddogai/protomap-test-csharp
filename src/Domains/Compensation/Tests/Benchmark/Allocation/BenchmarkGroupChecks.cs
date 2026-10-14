namespace Acme.Workforce.Compensation.Tests.Benchmark.Allocation;

using Acme.Workforce.Compensation.Benchmark.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class BenchmarkGroupChecks
{
    public static void BenchmarkGroupStartsInOpen()
    {
        var item = BenchmarkGroup.NewBenchmarkGroup("code-496", "name-669");
        Check.Equal(item.Status, BenchmarkGroupStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BenchmarkGroupMovesToInReview()
    {
        var item = BenchmarkGroup.NewBenchmarkGroup("code-496", "name-669");
        try
        {
            item.MoveTo(BenchmarkGroupStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BenchmarkGroupStatus.InReview, "status");
    }

    public static void BenchmarkGroupRejectsMovesFromArchived()
    {
        var item = BenchmarkGroup.NewBenchmarkGroup("code-496", "name-669");
        item.Status = BenchmarkGroupStatus.Archived;
        Check.Fails(() => item.MoveTo(BenchmarkGroupStatus.Open), "move");
    }

    public static void RunAll()
    {
        BenchmarkGroupStartsInOpen();
        BenchmarkGroupMovesToInReview();
        BenchmarkGroupRejectsMovesFromArchived();
    }
}
